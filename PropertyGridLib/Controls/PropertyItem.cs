using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PropertyGridLib.Attributes;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 自定义编辑器类型
    /// </summary>
    public enum CustomEditorType
    {
        None,
        FilePath,
        DirectoryPath,
        Collection,
        Expandable,
        Slider,
        DropDown,
        Formula,
        MultiSelect,
        Button,
        Dictionary,
        Color,
        /// <summary>布尔开关（ToggleSwitch）编辑器，见 [ToggleSwitch] 特性</summary>
        ToggleSwitch,
        /// <summary>多行文本（富文本查看）编辑器，见 [MultilineText] 特性</summary>
        MultilineText
    }

    /// <summary>
    /// 表示 PropertyGrid 中的单个属性项。
    /// 封装了反射信息、特性解析、值读写等核心逻辑。
    /// </summary>
    public class PropertyItem : IPropertyItem, INotifyPropertyChanged
    {
        private readonly PropertyInfo _propertyInfo;
        /// <summary>宿主对象。不是 readonly：同类型换对象时由 <see cref="RebindOwner"/> 原地换绑，
        /// 从而复用属性元数据与已生成的行控件（换设备/换产品是高频操作，整表反射重建很贵）。</summary>
        private object _owner;
        private readonly ICommand _resetCommand;
        private readonly int _maxDepth;
        private readonly int _currentDepth;
        private object _originalValue;

        // FormulaBound<T> 支持
        private readonly bool _isFormulaBound;
        private readonly Type _formulaBoundValueType;
        private readonly PropertyInfo _formulaBoundValueProp;
        private readonly PropertyInfo _formulaBoundFormulaProp;

        // [Button] 支持
        private string _buttonTextRaw;
        private string _buttonAction;
        private bool _buttonEnabled = true;

        public event PropertyChangedEventHandler PropertyChanged;

        #region 构造

        public PropertyItem(PropertyInfo propertyInfo, object owner, int maxDepth = 10, int currentDepth = 0)
        {
            _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _maxDepth = maxDepth;
            _currentDepth = currentDepth;
            _resetCommand = new RelayCommand(_ => ResetToOriginal(), _ => !IsReadOnly);

            // 解析特性
            UpdateLocalizedNames();
            IsReadOnly = propertyInfo.CanWrite == false || GetAttribute<ReadOnlyAttribute>()?.IsReadOnly == true;
            IsBrowsable = GetAttribute<BrowsableAttribute>()?.Browsable ?? true;

            // 检测 FormulaBound<T> 类型
            var propType = propertyInfo.PropertyType;
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(FormulaBound<>))
            {
                _isFormulaBound = true;
                _formulaBoundValueType = propType.GetGenericArguments()[0];
                _formulaBoundValueProp = propType.GetProperty("Value");
                _formulaBoundFormulaProp = propType.GetProperty("Formula");
            }

            // 记录初始值（用于 Reset）
            SaveOriginalValue();

            // 枚举值（FormulaBound 时基于内部类型 T）
            var enumType = _isFormulaBound ? _formulaBoundValueType : propertyInfo.PropertyType;
            if (enumType.IsEnum)
            {
                _enumType = enumType;
                EnumValues = Enum.GetValues(enumType);
                BuildEnumDisplayItems();
            }

            // 是否可展开（FormulaBound 不可展开）
            IsExpandable = _isFormulaBound ? CanExpand(_formulaBoundValueType) : CanExpand(propertyInfo.PropertyType);

            // 检测自定义编辑器
            DetectCustomEditor(propertyInfo);

            // 按钮属性没有"值/展开"语义，禁用展开
            if (CustomEditor == CustomEditorType.Button)
                IsExpandable = false;

            // 公式已有内容（宿主已绑定）：默认展开公式条，直接呈现「已绑定」状态，
            // 而不是停在原始值编辑器上、等用户去点绑定按钮。空公式保持默认收起。
            if (IsFormulaEnabled)
            {
                try
                {
                    if (!string.IsNullOrEmpty(FormulaString))
                        _isFormulaEditorOpen = true;
                }
                catch
                {
                    // 个别 FormulaBound 实例取值失败时保持默认收起，不影响属性项构建
                }
            }
        }

        #endregion

        #region 属性

        /// <summary>属性名称（代码中的名称）</summary>
        public string Name => _propertyInfo.Name;

        /// <summary>显示名称（[DisplayName] 特性指定，支持 IPropertyLocalization 动态变更）</summary>
        public string DisplayName
        {
            get => _displayName;
            private set { _displayName = value; OnPropertyChanged(nameof(DisplayName)); }
        }
        private string _displayName;

        /// <summary>分类（[Category] 特性指定，支持 IPropertyLocalization 动态变更）</summary>
        public string Category
        {
            get => _category;
            private set { _category = value; OnPropertyChanged(nameof(Category)); }
        }
        private string _category;

        /// <summary>排序用基准显示名（特性原文，不随语言变化）——语言切换只换显示，不换顺序。</summary>
        public string SortDisplayName => _baseDisplayName ?? DisplayName;
        /// <summary>排序用基准分类名（特性原文，不随语言变化）。</summary>
        public string SortCategory => _baseCategory ?? Category;
        private string _baseDisplayName;
        private string _baseCategory;

        /// <summary>描述（[Description] 特性指定，支持 IPropertyLocalization 动态变更）</summary>
        public string Description
        {
            get => _description;
            private set { _description = value; OnPropertyChanged(nameof(Description)); }
        }
        private string _description;

        /// <summary>是否只读（框架内置 [ReadOnly] 特性或 PropertyGrid.ReadOnly 网格级开关置定）；
        /// 各编辑器模板都绑定了此值，置 true 后所有编辑器变只读、重置按钮禁用</summary>
        public bool IsReadOnly { get; internal set; }

        /// <summary>是否可浏览显示</summary>
        public bool IsBrowsable { get; }

        /// <summary>属性类型</summary>
        public Type PropertyType => _propertyInfo.PropertyType;

        /// <summary>有效属性类型（FormulaBound&lt;T&gt; 时返回 T，否则返回 PropertyType）</summary>
        public Type EffectivePropertyType => _isFormulaBound ? _formulaBoundValueType : PropertyType;

        /// <summary>
        /// 类型提示文本，形如 “(int)”、“(string)”、“(SampleObject2)”。
        /// 仅供 UI 在公式绑定区等位置以淡色显示，提示当前属性的有效类型 T（FormulaBound 时为内部 T）。
        /// 复杂泛型签名做轻度简化：Dictionary&lt;string,int&gt; → (Dictionary&lt;string,int&gt;)，过长时由 UI 截断。
        /// </summary>
        public string TypeHint => "(" + GetFriendlyTypeName(EffectivePropertyType) + ")";

        /// <summary>生成可读类型名：泛型参数原样保留（如 List&lt;int&gt;），可空类型去 Nullable&lt;&gt; 外壳，加 ? 后缀</summary>
        private static string GetFriendlyTypeName(Type type)
        {
            if (type == null) return "object";

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlying = Nullable.GetUnderlyingType(type);
                return GetFriendlyTypeName(underlying) + "?";
            }

            if (type.IsGenericType)
            {
                var name = type.Name;
                var tickIdx = name.IndexOf('`');
                if (tickIdx > 0) name = name.Substring(0, tickIdx);
                var args = type.GetGenericArguments();
                return name + "<" + string.Join(", ", args.Select(GetFriendlyTypeName)) + ">";
            }

            return type.Name;
        }

        /// <summary>
        /// 复杂对象在属性格只读值区的显示文本（可展开对象行右侧、平铺行同此来源）：
        /// <list type="bullet">
        /// <item>重写了 <see cref="object.ToString"/> 的类型，尊重其自定义输出；</item>
        /// <item>否则优先取类型级 <see cref="DisplayNameAttribute"/>（可给配置类起业务名）；</item>
        /// <item>再回退友好类型名：去掉命名空间、展开泛型参数——
        /// 默认 ToString 输出的 <c>命名空间.类名</c>（如 Ext.FlowWork.Settings.AutoBackupOptions）又长又不可读。</item>
        /// </list>
        /// 未重写 ToString 的结构体（继承自 ValueType.ToString）同样回退友好名。
        /// </summary>
        private static string GetComplexValueDisplay(object value)
        {
            var type = value.GetType();
            // 枚举：ToString 返回成员名（如 End），保持原样不做类型名回退
            if (type.IsEnum) return value.ToString() ?? string.Empty;
            var toStringMethod = type.GetMethod(nameof(object.ToString), Type.EmptyTypes);
            if (toStringMethod != null
                && toStringMethod.DeclaringType != typeof(object)
                && toStringMethod.DeclaringType != typeof(ValueType))
            {
                return value.ToString() ?? string.Empty;
            }

            var typeDisplayName = TypeDescriptor.GetAttributes(type)
                .OfType<DisplayNameAttribute>()
                .FirstOrDefault(a => !string.IsNullOrEmpty(a.DisplayName))?.DisplayName;
            return string.IsNullOrEmpty(typeDisplayName)
                ? GetFriendlyTypeName(type)
                : typeDisplayName;
        }

        /// <summary>
        /// 浮点数值的显示格式化：按有效数字输出以消除二进制舍入噪声，且不损失常规业务精度。
        /// <paramref name="generalFormat"/> 为有效数字格式（double 用 G15、float 用 G7）；
        /// 当结果落到科学计数法（含 'E'/'e'，如极小值 1.5E-05）时改用
        /// <paramref name="fixedFormat"/> 定点格式，避免配置面板里出现 E 记数写法。
        /// 只影响显示文本，底层存储与序列化保持原始双精度值不变。
        /// </summary>
        private static string FormatFloatingPoint(IFormattable value, string generalFormat, string fixedFormat)
        {
            var text = value.ToString(generalFormat, CultureInfo.CurrentCulture);
            if (text.IndexOf('E') >= 0 || text.IndexOf('e') >= 0)
            {
                // 定点格式位数有限（double 15 位小数 / float 6 位小数），对极小值会整串截断成 0
                // （如 1e-16 → "0.000000000000000"），把非零值显示成 0、误导用户；
                // 这种情况保留科学计数写法（1E-16）反而更准确。
                var fixedText = value.ToString(fixedFormat, CultureInfo.CurrentCulture);
                text = IsAllZeroDigits(fixedText) ? text : fixedText;
            }
            return text;
        }

        /// <summary>定点文本里是否没有任何 1-9 的有效数字（即该值被定点位数截断成了 0）</summary>
        private static bool IsAllZeroDigits(string text)
        {
            foreach (var ch in text)
                if (ch >= '1' && ch <= '9') return false;
            return true;
        }

        /// <summary>枚举值列表</summary>
        public Array EnumValues { get; }

        private Type _enumType;

        /// <summary>
        /// 枚举下拉显示项：枚举成员的 [Description] 原文经全局翻译钩子取词（中文原文作 key），
        /// 无特性时回退成员名。随语言切换刷新（UpdateLocalization）。
        /// </summary>
        public List<string> EnumDisplayItems { get; private set; }

        /// <summary>
        /// 枚举选中项（显示文本形态，供下拉双向绑定）：
        /// get 把当前枚举值映射成显示文本；set 把显示文本映射回枚举值写入 <see cref="Value"/>。
        /// </summary>
        public string EnumSelectedItem
        {
            get
            {
                if (_enumType == null) return null;
                var v = Value;
                if (v == null || !_enumType.IsInstanceOfType(v)) return null;
                return EnumToDisplay(_enumType, v);
            }
            set
            {
                if (_enumType == null || string.IsNullOrEmpty(value)) return;
                if (TryDisplayToEnum(_enumType, value, out var e))
                {
                    Value = e;
                    OnPropertyChanged(nameof(EnumSelectedItem));
                }
            }
        }

        /// <summary>枚举值 → 显示文本：[Description] 原文经翻译，无特性回退成员名。</summary>
        private static string EnumToDisplay(Type enumType, object value)
        {
            var name = Enum.GetName(enumType, value);
            var member = enumType.GetMember(name ?? "").FirstOrDefault();
            var desc = member?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
            return T(desc) ?? name;
        }

        /// <summary>显示文本 → 枚举值：按 [Description]（翻译前后都比对）匹配，失败回退成员名，再失败返回 false。</summary>
        private static bool TryDisplayToEnum(Type enumType, string display, out object value)
        {
            foreach (Enum e in Enum.GetValues(enumType))
            {
                var member = enumType.GetMember(e.ToString()).FirstOrDefault();
                var desc = member?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
                var text = T(desc) ?? e.ToString();
                if (string.Equals(display, text, StringComparison.Ordinal))
                {
                    value = e;
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>构建枚举显示项列表（构造与语言切换时调用）。</summary>
        private void BuildEnumDisplayItems()
        {
            var items = new List<string>();
            foreach (Enum e in Enum.GetValues(_enumType))
                items.Add(EnumToDisplay(_enumType, e));
            EnumDisplayItems = items;
        }

        /// <summary>是否可展开为子属性</summary>
        public bool IsExpandable { get; }

        /// <summary>
        /// 缩进层级（0 = 顶层属性行）。扁平化列表（BuildFlatRows）递归展开子属性时写入；
        /// 行模板用 <see cref="RowMargin"/> 呈现层级缩进。
        /// 所有行（父行/子属性行）等高，VirtualizingStackPanel 估算才稳定，滚动条不再抖动。
        /// </summary>
        public int Level { get; internal set; }

        /// <summary>行 Margin（左缩进随层级递增，上下 1px 保留行间分隔）</summary>
        public Thickness RowMargin => new Thickness(Level * 18, 1, 0, 1);

        /// <summary>Pro 卡片式行的 Margin（保留卡片 14px 外边距，缩进随层级递增）</summary>
        public Thickness ProRowMargin => new Thickness(14 + Level * 20, 0, 14, 10);

        /// <summary>自定义编辑器类型</summary>
        public CustomEditorType CustomEditor { get; private set; }

        /// <summary>
        /// 宿主通过 PropertyGrid.CustomEditors 注册的自定义编辑器 DataTemplate。
        /// 非 null 时 EditorTemplateSelector 优先使用此模板，忽略 CustomEditor 枚举。
        /// </summary>
        public DataTemplate CustomEditorTemplate { get; private set; }

        /// <summary>
        /// 宿主自定义编辑器对应的 Attribute 实例。
        /// 模板可以通过 Binding="{Binding CustomEditorAttribute}" 拿到宿主定义的 Attribute，
        /// 再从中读取 Units、DefaultUnit 等参数。
        /// </summary>
        public Attribute CustomEditorAttribute { get; private set; }

        /// <summary>文件过滤器（FilePath 特性）</summary>
        public string FileFilter { get; private set; }

        /// <summary>滑块最小值（NumberSlider 特性）</summary>
        public double SliderMinimum { get; private set; } = 0;

        /// <summary>滑块最大值（NumberSlider 特性）</summary>
        public double SliderMaximum { get; private set; } = 100;

        /// <summary>滑块步长（NumberSlider 特性）</summary>
        public double SliderStep { get; private set; } = 1;

        /// <summary>是否在滑块旁显示数字输入框（NumberSlider.ShowNumberBox，与滑块双向同步）；false 时显示只读数值标签</summary>
        public bool SliderShowNumberBox { get; private set; } = false;

        /// <summary>开关打开（true）状态文字：[ToggleSwitch] 的 OnText；null = 库默认（随语言「开/ON」）；空 = 不显示</summary>
        public string ToggleOnText { get; private set; }

        /// <summary>开关关闭（false）状态文字：[ToggleSwitch] 的 OffText；null = 库默认（随语言「关/OFF」）；空 = 不显示</summary>
        public string ToggleOffText { get; private set; }

        /// <summary>
        /// 开关编辑器旁显示的状态文字（ToggleSwitch 特性）：值为 true 时取 <see cref="ToggleOnText"/>，
        /// 否则取 <see cref="ToggleOffText"/>。取值规则：null → 库内置文字（随语言「开/ON」「关/OFF」，
        /// 经 TextResolver 翻译）；空字符串 → 不显示文字；自定义文字经 TextResolver 翻译。
        /// 语言切换后由 UpdateLocalization() 重新通知。
        /// </summary>
        public string ToggleStateText
        {
            get
            {
                var isOn = Equals(Value, true);
                var raw = isOn ? ToggleOnText : ToggleOffText;
                // null = 未指定 → 库内置默认（随语言）；空字符串 = 明确不显示
                if (raw == null)
                    raw = LocalizationManager.GetString(isOn ? "ToggleOn" : "ToggleOff");
                return T(raw) ?? string.Empty;
            }
        }

        /// <summary>状态文字是否可见（ToggleSwitch 特性，On/Off 文字均为空时折叠文字）</summary>
        public Visibility ToggleStateTextVisibility
            => string.IsNullOrEmpty(ToggleStateText) ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>下拉列表标准值（TypeConverter 提供）</summary>
        public List<object> StandardValues { get; private set; }

        /// <summary>下拉列表是否排他（不允许自定义输入）</summary>
        public bool IsStandardValuesExclusive { get; private set; }

        /// <summary>是否启用公式绑定</summary>
        public bool IsFormulaEnabled { get; private set; }

        /// <summary>公式编辑器（覆盖条）是否展开显示。默认收起，点击绑定按钮展开（覆盖在编辑器行上），再点收起。
        /// 覆盖显示不参与行布局 → 行高恒定，虚拟化估算稳定（滚动条不抖动）。</summary>
        public bool IsFormulaEditorOpen
        {
            get => _isFormulaEditorOpen;
            set
            {
                if (_isFormulaEditorOpen == value) return;
                _isFormulaEditorOpen = value;
                OnPropertyChanged(nameof(IsFormulaEditorOpen));
            }
        }
        private bool _isFormulaEditorOpen;

        /// <summary>切换公式编辑器覆盖条显示/隐藏</summary>
        public ICommand ToggleFormulaEditorCommand =>
            _toggleFormulaEditorCommand ??= new RelayCommand(_ => IsFormulaEditorOpen = !IsFormulaEditorOpen);
        private ICommand _toggleFormulaEditorCommand;

        /// <summary>公式字符串</summary>
        public string FormulaString
        {
            get
            {
                if (_isFormulaBound)
                {
                    var bound = _propertyInfo.GetValue(_owner);
                    return bound != null ? (string)_formulaBoundFormulaProp.GetValue(bound) : string.Empty;
                }
                return _formulaString;
            }
            set
            {
                if (_isFormulaBound)
                {
                    var bound = _propertyInfo.GetValue(_owner);
                    if (bound != null)
                    {
                        // TwoWay 绑定在行控件复用/换绑宿主时会把控件当前值回推；
                        // 非公式属性在 Value setter 里有同样的相等性保护，公式这里也不能漏：
                        // 不回推相同值，避免"切到同类型条目时把上一项的公式写进新宿主"。
                        var current = (string)_formulaBoundFormulaProp.GetValue(bound);
                        if (string.Equals(current, value, StringComparison.Ordinal)) return;

                        _formulaBoundFormulaProp.SetValue(bound, value);
                        OnPropertyChanged(nameof(FormulaString));
                    }
                    return;
                }
                if (_formulaString != value)
                {
                    _formulaString = value;
                    OnPropertyChanged(nameof(FormulaString));
                }
            }
        }
        private string _formulaString = string.Empty;

        /// <summary>公式树数据提供者（从 FormulaEditorAttribute 实例化）</summary>
        public IFormulaTreeProvider FormulaTreeProvider { get; private set; }

        /// <summary>多选列表数据提供者（从 MultiSelectAttribute 实例化）</summary>
        public IMultiSelectProvider MultiSelectProvider { get; private set; }

        /// <summary>按钮显示文本（[Button] 特性）；为空时回退到 DisplayName（随多语言切换而变化）</summary>
        public string ButtonText => string.IsNullOrEmpty(_buttonTextRaw) ? DisplayName : T(_buttonTextRaw);

        /// <summary>属性值（直接读写对象的属性）</summary>
        public object Value
        {
            get
            {
                try
                {
                    // Brush 子属性（Color/Opacity 等）：父属性写回的是复制出的新 Brush，
                    // 子属性必须始终读"父属性当前值"，否则写回后子行仍显示旧实例的值
                    if (Parent != null && Parent.Value is SolidColorBrush)
                        return _propertyInfo.GetValue(Parent.Value);
                    if (_isFormulaBound)
                    {
                        var bound = _propertyInfo.GetValue(_owner);
                        return bound != null ? _formulaBoundValueProp.GetValue(bound) : null;
                    }
                    return _propertyInfo.GetValue(_owner);
                }
                catch
                {
                    // 个别属性 getter 反射取值异常（Type 元数据属性等）：返回 null，
                    // 不让"展开/折叠全部"等递归遍历崩溃（CanExpand 已拦截 Type，此处兜底）
                    return null;
                }
            }
            set
            {
                try
                {
                    // 相等性检查：值未变则不写回、不触发 ValueChanged。
                    // 背景：TwoWay 绑定在「虚拟化容器复用 / 绑定建立」时会把控件当前值推回源。
                    // 实测每次重建属性列表都会误触发一次 bool 属性（如 AllowDrop）的"变更"事件，
                    // 宿主（设计器）会据此无意义地把同一个值反复写回 XAML。
                    // 值真正变化时才继续往下走。
                    if (TryGetCurrentValue(out var currentValue) && ValuesEqual(currentValue, value))
                        return;

                    // Brush 子属性（Color/Opacity 等）写回：原地改 Brush 实例会污染共享画刷
                    // （未显式设置 Background 时 TextBox 默认样式的画刷是全局共享，改一处全程序输入框变色）。
                    // 正确做法：复制父属性当前 Brush → 应用本子属性修改 → 写回父属性（原实例不动）。
                    if (Parent != null && _owner is SolidColorBrush && Parent.Value is SolidColorBrush parentBrush)
                    {
                        var copy = new SolidColorBrush(parentBrush.Color)
                        {
                            Opacity = parentBrush.Opacity
                        };
                        if (_propertyInfo.Name == "Color")
                            copy.Color = (Color)value;
                        else if (_propertyInfo.Name == "Opacity")
                            copy.Opacity = (double)value;
                        else
                            _propertyInfo.SetValue(copy, value);   // 其他子属性兜底

                        Parent._propertyInfo.SetValue(Parent._owner, copy);
                        OnPropertyChanged(nameof(Value));
                        OnPropertyChanged(nameof(ValueString));
                        OnPropertyChanged(nameof(IsDefault));
                        OnPropertyChanged(nameof(IsModified));
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    if (_isFormulaBound)
                    {
                        var bound = _propertyInfo.GetValue(_owner);
                        if (bound != null)
                        {
                            _formulaBoundValueProp.SetValue(bound, value);
                        }
                        OnPropertyChanged(nameof(Value));
                        OnPropertyChanged(nameof(ValueString));
                        OnPropertyChanged(nameof(IsDefault));
                        OnPropertyChanged(nameof(IsModified));
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    if (value is string strVal && _propertyInfo.PropertyType != typeof(string))
                    {
                        var converter = TypeDescriptor.GetConverter(_propertyInfo.PropertyType);
                        if (converter.CanConvertFrom(typeof(string)))
                        {
                            var converted = converter.ConvertFromString(strVal);
                            _propertyInfo.SetValue(_owner, converted);
                        }
                    }
                    else if (value != null && _propertyInfo.PropertyType.IsInstanceOfType(value))
                    {
                        _propertyInfo.SetValue(_owner, value);
                    }
                    else if (value == null)
                    {
                        _propertyInfo.SetValue(_owner, null);
                    }
                    else if (value is IConvertible && _propertyInfo.PropertyType.IsPrimitive)
                    {
                        var converted = Convert.ChangeType(value, _propertyInfo.PropertyType);
                        _propertyInfo.SetValue(_owner, converted);
                    }
                    else
                    {
                        var converter = TypeDescriptor.GetConverter(_propertyInfo.PropertyType);
                        if (converter.CanConvertFrom(value.GetType()))
                        {
                            var converted = converter.ConvertFrom(value);
                            _propertyInfo.SetValue(_owner, converted);
                        }
                    }

                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(ValueString));
                    OnPropertyChanged(nameof(IsDefault));
                    OnPropertyChanged(nameof(IsModified));
                    OnPropertyChanged(nameof(ChildProperties));
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                catch
                {
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(ValueString));
                    OnPropertyChanged(nameof(IsDefault));
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        /// <summary>属性值的字符串表示（用于 TextBox 双向绑定）</summary>
        public string ValueString
        {
            get
            {
                if (Value == null) return string.Empty;

                if (Value is IDictionary dict)
                {
                    return LocalizationManager.GetString("ItemsCount", dict.Count);
                }

                if (Value is ICollection collection)
                {
                    return LocalizationManager.GetString("ItemsCount", collection.Count);
                }

                // Color / SolidColorBrush → 不透明时输出 #RRGGBB，有透明度时输出 #AARRGGBB
                if (Value is Color c)
                {
                    return c.A == 255
                        ? $"#{c.R:X2}{c.G:X2}{c.B:X2}"
                        : $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
                }
                if (Value is SolidColorBrush scb)
                {
                    var cc = scb.Color;
                    return cc.A == 255
                        ? $"#{cc.R:X2}{cc.G:X2}{cc.B:X2}"
                        : $"#{cc.A:X2}{cc.R:X2}{cc.G:X2}{cc.B:X2}";
                }

                // 浮点数值：抹掉二进制舍入噪声，只改显示、不动真实值。
                // 默认 ToString 给的是「最短往返表示」——会把噪声原样吐出：
                // 滑块按 0.1 步进到 1.2 时 double 实为 1.2000000000000002，
                // 数值框/滑块标签便显示成 1.2000000000000002（框宽不足还被裁成 1.200000000000）。
                // G15（double，15 位有效数字）/ G7（float，7 位有效数字）足以覆盖常规业务值精度，
                // 同时把 0.1+0.2→0.3、12*0.1→1.2 这类噪声归位；G 格式自带去尾零。
                if (Value is double dbl) return FormatFloatingPoint(dbl, "G15", "0.###############");
                if (Value is float flt) return FormatFloatingPoint(flt, "G7", "0.######");

                return GetComplexValueDisplay(Value);
            }
            set
            {
                // 相等性检查（与 Value setter 同理）：
                // 绑定建立 / 虚拟化容器复用时，TwoWay 绑定会把控件当前值推回 ValueString。
                // 实测传入值与当前值完全相同（如 "False" vs False）却仍写入并触发 ValueChanged，
                // 导致宿主（设计器）在没有任何用户操作的情况下反复把同一个值写回 XAML
                // （压测 30 秒内误触发上百次，涉及 bool 与各类枚举属性）。
                // 这里先把传入字符串按目标类型转换，再与当前值比较，相同则直接返回。
                if (TryGetCurrentValue(out var vsCurrent))
                {
                    var vsTargetType = _isFormulaBound ? _formulaBoundValueType : _propertyInfo.PropertyType;
                    try
                    {
                        var vsConv = TypeDescriptor.GetConverter(vsTargetType);
                        if (vsConv != null && vsConv.CanConvertFrom(typeof(string)))
                        {
                            var vsConverted = vsConv.ConvertFromString(value ?? string.Empty);
                            if (ValuesEqual(vsCurrent, vsConverted))
                                return;
                        }
                    }
                    catch { /* 转换失败则按原逻辑处理 */ }
                }

                if (_isFormulaBound)
                {
                    var effectiveType = _formulaBoundValueType;
                    if (string.IsNullOrEmpty(value) && effectiveType.IsValueType && Nullable.GetUnderlyingType(effectiveType) == null)
                        return;
                    try
                    {
                        var converter = TypeDescriptor.GetConverter(effectiveType);
                        if (converter.CanConvertFrom(typeof(string)))
                        {
                            var converted = converter.ConvertFromString(value);
                            var bound = _propertyInfo.GetValue(_owner);
                            if (bound != null)
                            {
                                _formulaBoundValueProp.SetValue(bound, converted);
                                OnPropertyChanged(nameof(Value));
                                OnPropertyChanged(nameof(ValueString));
                                OnPropertyChanged(nameof(IsDefault));
                                OnPropertyChanged(nameof(IsModified));
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                            }
                        }
                    }
                    catch { }
                    return;
                }

                if (string.IsNullOrEmpty(value) && _propertyInfo.PropertyType.IsValueType)
                    return;

                if (Value is IDictionary || Value is ICollection)
                    return;

                try
                {
                    var converter = TypeDescriptor.GetConverter(_propertyInfo.PropertyType);
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        var converted = converter.ConvertFromString(value);
                        _propertyInfo.SetValue(_owner, converted);
                        OnPropertyChanged(nameof(Value));
                        OnPropertyChanged(nameof(IsDefault));
                        OnPropertyChanged(nameof(IsModified));
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>是否有初始值（用于 Reset 按钮显示）</summary>
        public bool HasDefaultValue => true;

        /// <summary>当前值是否等于初始值</summary>
        public bool IsDefault
        {
            get
            {
                if (_isFormulaBound)
                {
                    var current = Value;
                    if (_originalValue == null && current == null) return true;
                    if (_originalValue == null || current == null) return false;
                    return _originalValue.Equals(current);
                }
                if (_originalValue == null && Value == null) return true;
                if (_originalValue == null || Value == null) return false;
                return _originalValue.Equals(Value);
            }
        }

        /// <summary>安全读取当前值（反射读属性可能抛异常，失败时无法比较）</summary>
        private bool TryGetCurrentValue(out object value)
        {
            try
            {
                value = Value;
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// 判断新值是否与当前值等价。需处理 null、装箱相等，以及编辑器常见的
        /// 「字符串 → 目标类型」提交（如 "1" 与 1、"false" 与 false 视为相同）。
        /// </summary>
        private bool ValuesEqual(object current, object incoming)
        {
            if (current == null && incoming == null) return true;
            if (current == null || incoming == null) return false;
            if (Equals(current, incoming)) return true;

            if (incoming is string s)
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(_propertyInfo.PropertyType);
                    if (converter != null && converter.CanConvertFrom(typeof(string)))
                    {
                        var converted = converter.ConvertFromString(s);
                        if (Equals(current, converted)) return true;
                    }
                }
                catch { /* 转换失败按不等处理 */ }

                try
                {
                    if (_propertyInfo.PropertyType != typeof(string))
                    {
                        var converted = Convert.ChangeType(s, _propertyInfo.PropertyType);
                        if (Equals(current, converted)) return true;
                    }
                }
                catch { /* 转换失败按不等处理 */ }
            }
            return false;
        }

        /// <summary>属性值是否已被修改（用于控制 R 按钮显示）</summary>
        public bool IsModified => !IsDefault;

        /// <summary>值变更事件</summary>
        public event EventHandler ValueChanged;

        /// <summary>重置命令</summary>
        public ICommand ResetCommand => _resetCommand;

        /// <summary>浏览文件命令</summary>
        public ICommand BrowseFileCommand => _browseFileCommand = new RelayCommand(_ => BrowseFile());
        private ICommand _browseFileCommand;

        /// <summary>浏览目录命令</summary>
        public ICommand BrowseDirectoryCommand => _browseDirectoryCommand = new RelayCommand(_ => BrowseDirectory());
        private ICommand _browseDirectoryCommand;

        /// <summary>编辑集合命令</summary>
        public ICommand EditCollectionCommand => _editCollectionCommand = new RelayCommand(_ => EditCollection());
        private ICommand _editCollectionCommand;

        /// <summary>按钮点击命令（[Button] 特性）；仅当属性显式标注 [ReadOnly(true)] 时禁用</summary>
        public ICommand ButtonClickCommand => _buttonClickCommand ?? (_buttonClickCommand = new RelayCommand(_ => OnButtonClicked(), _ => _buttonEnabled));
        private ICommand _buttonClickCommand;

        /// <summary>按钮被点击时触发（在调用宿主方法之后）</summary>
        public event EventHandler<PropertyButtonClickEventArgs> ButtonClicked;

        /// <summary>子属性列表（用于 Expandable 类型的展开显示）</summary>
        public List<PropertyItem> ChildProperties
        {
            get
            {
                if (_childProperties == null && IsExpandable && Value != null)
                {
                    LoadChildProperties();
                }
                return _childProperties;
            }
        }
        private List<PropertyItem> _childProperties;

        /// <summary>父属性项（子属性时非 null）。Brush 子属性编辑需要它：写回时复制新 Brush 赋给父属性，
        /// 避免原地修改共享画刷（未显式设置 Background 时 TextBox 默认样式的画刷是全局共享的）。</summary>
        internal PropertyItem Parent { get; set; }

        private bool _isExpanded;
        /// <summary>是否已展开</summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));

                    if (value && _childProperties == null && IsExpandable && Value != null)
                    {
                        LoadChildProperties();
                        OnPropertyChanged(nameof(ChildProperties));
                    }
                }
            }
        }

        #endregion

        #region 方法

        /// <summary>保存初始值（反射取值失败时降级为 null，不抛异常 —— 避免展开/折叠全部时
        /// 个别属性的 getter 或反射路径抛异常导致整个列表重建崩溃）</summary>
        private void SaveOriginalValue()
        {
            try
            {
                var currentValue = _propertyInfo.GetValue(_owner);
                if (currentValue == null)
                {
                    _originalValue = null;
                    return;
                }

                // [field:NonSerialized] 标记的属性跳过深拷贝
                if (IsNonSerializedProperty())
                {
                    _originalValue = null;
                    return;
                }

                if (_isFormulaBound)
                {
                    var val = _formulaBoundValueProp.GetValue(currentValue);
                    _originalValue = val != null && !val.GetType().IsValueType && val.GetType() != typeof(string)
                        ? DeepClone(val) : val;
                    return;
                }

                if (_propertyInfo.PropertyType.IsValueType || _propertyInfo.PropertyType == typeof(string))
                {
                    _originalValue = currentValue;
                }
                else if (typeof(IList).IsAssignableFrom(_propertyInfo.PropertyType)
                         || typeof(IDictionary).IsAssignableFrom(_propertyInfo.PropertyType))
                {
                    // 集合可能被原地增删，需要深拷贝才能准确 Reset
                    _originalValue = DeepClone(currentValue);
                }
                else
                {
                    // 其余引用类型（Brush/Transform/Geometry/Style 等 WPF 对象）直接存引用：
                    // 属性编辑器对这类属性都是整体赋值替换（不原地修改对象），Reset 赋回原引用即可。
                    // 此前对每个子属性做 JSON 深拷贝（Serialize+Deserialize 往返）是展开/折叠全部时
                    // 构造几百个子属性卡顿的根源。
                    _originalValue = currentValue;
                }
            }
            catch
            {
                // 个别属性取值/克隆失败：原值不可用（该属性"重置"退化），但绝不中断整个面板构建
                _originalValue = null;
            }
        }

        /// <summary>重置为初始值</summary>
        public void ResetToOriginal()
        {
            if (_isFormulaBound)
            {
                var bound = _propertyInfo.GetValue(_owner);
                if (bound != null)
                {
                    _formulaBoundValueProp.SetValue(bound, _originalValue);
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(ValueString));
                    OnPropertyChanged(nameof(IsDefault));
                    OnPropertyChanged(nameof(IsModified));
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                return;
            }

            if (_originalValue == null)
            {
                Value = null;
            }
            else if (_propertyInfo.PropertyType.IsValueType || _propertyInfo.PropertyType == typeof(string))
            {
                Value = _originalValue;
            }
            else
            {
                var cloned = DeepClone(_originalValue);
                _propertyInfo.SetValue(_owner, cloned);
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(ValueString));
                OnPropertyChanged(nameof(IsDefault));
                OnPropertyChanged(nameof(IsModified));
                OnPropertyChanged(nameof(ChildProperties));
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>重置为初始值（别名）</summary>
        public void ResetToDefault()
        {
            ResetToOriginal();
        }

        /// <summary>刷新值通知</summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(ValueString));
            OnPropertyChanged(nameof(IsDefault));
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(ChildProperties));
            OnPropertyChanged(nameof(IsLink));
            OnPropertyChanged(nameof(BoolValue));
            // 公式文本也是"从宿主现读"的 live 值：同类型换对象（集合编辑器切换条目）
            // 走 RebindOwner→Refresh，漏发这一条会让公式框继续显示上一项的公式，
            // 现场看起来就是"改了一项的绑定，另一项也变了"。
            OnPropertyChanged(nameof(FormulaString));
        }

        #region 值形态扩展（链接 / 布尔视图）

        /// <summary>
        /// 当前值是一个可访问链接（http / https / ftp / mailto）。
        /// 链接编辑器模板据此把字符串渲染成蓝色可点击文本，点击用系统默认程序打开。
        /// </summary>
        public bool IsLink
        {
            get
            {
                var text = ValueString;
                if (string.IsNullOrWhiteSpace(text)) return false;
                if (!Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)) return false;
                return uri.Scheme == Uri.UriSchemeHttp
                    || uri.Scheme == Uri.UriSchemeHttps
                    || uri.Scheme == Uri.UriSchemeFtp
                    || uri.Scheme == Uri.UriSchemeMailto;
            }
        }

        /// <summary>
        /// 当前值的布尔视图：勾选式属性行（[图5] 那种「复选框 + 标题 + 说明」）直接双向绑这个。
        /// 直接绑 <see cref="Value"/>（object）到 CheckBox.IsChecked 会因类型不可转而不生效。
        /// </summary>
        public bool BoolValue
        {
            get => Value is bool b && b;
            set => Value = value;
        }

        private ICommand _openLinkCommand;

        /// <summary>打开链接（系统默认浏览器/邮件客户端）。链接编辑器模板绑到按钮命令。</summary>
        public ICommand OpenLinkCommand =>
            _openLinkCommand ?? (_openLinkCommand = new RelayCommand(_ => OpenLink(), _ => IsLink));

        private void OpenLink()
        {
            var text = ValueString;
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(text.Trim()) { UseShellExecute = true });
            }
            catch
            {
                // 无默认程序 / 被策略拦截时静默失败：属性面板不弹框打断现场
            }
        }

        #endregion

        #region 关键属性标记 / 分组首行

        private bool _highlightResolved;
        private bool _isHighlighted;

        /// <summary>
        /// [Highlight] 标记：true = 关键属性。
        /// 属性面板据此打强调标记（基础网格标题旁的小圆点 / 表单式属性块左侧的竖线）。
        /// </summary>
        public bool IsHighlighted
        {
            get
            {
                if (!_highlightResolved) ResolveHighlight();
                return _isHighlighted;
            }
        }

        /// <summary>[Highlight(Tip = "...")] 指定的悬停提示；为空时标记不挂 ToolTip。</summary>
        public string HighlightTip { get; private set; }

        private void ResolveHighlight()
        {
            _highlightResolved = true;
            var attr = GetAttribute<Attributes.HighlightAttribute>();
            if (attr == null) return;
            _isHighlighted = true;
            HighlightTip = attr.Tip;
        }

        private bool _isGroupFirst;

        /// <summary>
        /// 是否为其所属分类的第一项（表单式分组排布用，由 PropertyGridPro 在压平行集合时写入）。
        /// 为 true 时左侧分类列渲染分类名，其余行左列留空 ——
        /// 这样"分类名与第一条属性同一行"，不需要往列表里插独立的分类头行。
        /// </summary>
        public bool IsGroupFirst
        {
            get => _isGroupFirst;
            internal set
            {
                if (_isGroupFirst == value) return;
                _isGroupFirst = value;
                OnPropertyChanged(nameof(IsGroupFirst));
            }
        }

        #region 图像预览

        private ImageSource _imagePreview;
        private string _imageInfo;
        private object _imageCacheFor;
        private bool _imageCacheValid;

        /// <summary>
        /// 该属性的值是不是图像：内置识别 ImageSource / 图像文件字节流，
        /// 以及宿主用 <see cref="ImagePreviewRegistry"/> 登记过的自定义图像类型（如 GenImage）。
        /// </summary>
        public bool IsImage
        {
            get
            {
                var value = Value;
                if (value != null) return ImagePreviewRegistry.CanPreview(value);
                // 值为 null：按"声明的类型"判断。属性声明成图像类型、当前还没图时，
                // 面板应显示"无预览"占位，而不是退化成一个空的可展开行。
                return ImagePreviewRegistry.CanPreviewType(EffectivePropertyType);
            }
        }

        /// <summary>
        /// 缩略图。按"值对象引用"缓存：同一次刷新里反复取值不会重复解码，
        /// 换图（Value 变成新对象）才重新转换。转换失败返回 null，面板显示"无预览"。
        /// </summary>
        public ImageSource ImagePreview
        {
            get
            {
                var current = Value;
                if (_imageCacheValid && ReferenceEquals(_imageCacheFor, current)) return _imagePreview;

                _imageCacheValid = true;
                _imageCacheFor = current;
                _imagePreview = ImagePreviewRegistry.Create(current, out var info);
                _imageInfo = info;
                return _imagePreview;
            }
        }

        /// <summary>图像说明（尺寸 / 像素格式等，由登记表或 <see cref="IImagePreviewSource"/> 提供）。</summary>
        public string ImageInfo
        {
            get
            {
                _ = ImagePreview;   // 说明与缩略图同一次转换产出
                return _imageInfo;
            }
        }

        private ICommand _previewImageCommand;

        /// <summary>打开大图预览窗口（缩略图点击）。</summary>
        public ICommand PreviewImageCommand =>
            _previewImageCommand ?? (_previewImageCommand = new RelayCommand(_ => ShowImagePreview(), _ => IsImage));

        private void ShowImagePreview()
        {
            var image = ImagePreview;
            if (image == null) return;
            try
            {
                ImagePreviewWindow.ShowPreview(image, DisplayName, ImageInfo);
            }
            catch
            {
                // 预览窗口打不开不影响属性面板
            }
        }

        #endregion

        #region 编辑器装饰件（[EditorChrome]）

        private Attributes.EditorChromeAttribute _chrome;
        private bool _chromeResolved;

        private Attributes.EditorChromeAttribute Chrome
        {
            get
            {
                if (!_chromeResolved)
                {
                    _chromeResolved = true;
                    _chrome = GetAttribute<Attributes.EditorChromeAttribute>();
                }
                return _chrome;
            }
        }

        /// <summary>[EditorChrome(ShowReset=false)] → 这一行不显示重置按钮。</summary>
        public bool ShowResetButton => Chrome?.ShowReset ?? true;

        /// <summary>[EditorChrome(ResetText="默认")] → 重置按钮上的字，默认 "R"。</summary>
        public string ResetText
        {
            get
            {
                var text = Chrome?.ResetText;
                return string.IsNullOrEmpty(text) ? "R" : text;
            }
        }

        /// <summary>[EditorChrome(ResetToolTip="...")] → 重置按钮悬停文案；未指定时取本地化默认值。</summary>
        public string ResetToolTip
        {
            get
            {
                var tip = Chrome?.ResetToolTip;
                if (!string.IsNullOrEmpty(tip)) return tip;
                try { return Localization.LocalizationManager.GetString("ResetToDefault"); }
                catch { return null; }
            }
        }

        /// <summary>[EditorChrome(ShowTypeHint=true/false)] → 覆盖该行的类型提示开关；null = 跟随全局。</summary>
        public bool? ShowTypeHintOverride => Chrome?.ShowTypeHint;

        #endregion

        private bool _isGroupLast;

        /// <summary>
        /// 是否为其所属分类的最后一项（分类"卡片"样式的收口用）：
        /// 为真时该行画下圆角与下边框，把一组属性收成一个圆角卡片。
        /// 由 <c>PropertyGrid.BuildFlatRows</c> 在压平行集合时写入。
        /// </summary>
        public bool IsGroupLast
        {
            get => _isGroupLast;
            internal set
            {
                if (_isGroupLast == value) return;
                _isGroupLast = value;
                OnPropertyChanged(nameof(IsGroupLast));
            }
        }

        private bool _showGroupSeparator;

        /// <summary>
        /// 该行顶部要不要画一条"分类之间的横向分隔线"。
        /// = 分组首行 且 不是第一个分组（第一个分组与页头之间已有页头自带的分隔线，不重复画）。
        /// 由 PropertyGridPro 在压平行集合时写入，模板只负责显示。
        /// </summary>
        public bool ShowGroupSeparator
        {
            get => _showGroupSeparator;
            internal set
            {
                if (_showGroupSeparator == value) return;
                _showGroupSeparator = value;
                OnPropertyChanged(nameof(ShowGroupSeparator));
            }
        }

        #endregion

        /// <summary>
        /// 换绑宿主对象（同类型快速刷新用）：
        /// 属性元数据（PropertyInfo / 特性 / 枚举表 / 自定义编辑器）全部与类型绑定，换实例时不需要动；
        /// 只需换取值/写值的宿主，并重算「原值基线」（Reset 与 IsModified 的依据）、
        /// 丢掉从旧宿主的值上反射出来的子属性缓存。
        ///
        /// 由 <c>PropertyGrid</c> 在「新旧 SelectedObject 类型相同且没有展开行」时调用，
        /// 宿主侧无感知：仍然只是给 SelectedObject 赋新对象。
        /// </summary>
        internal void RebindOwner(object owner)
        {
            if (owner == null || ReferenceEquals(owner, _owner)) return;

            _owner = owner;
            _childProperties = null;   // 子属性是从旧宿主的属性值上反射出来的，必须按新宿主重来
            _isExpanded = false;       // 快速刷新只在"无展开行"时走，这里保持状态一致
            SaveOriginalValue();       // 原值基线跟着新宿主重取，否则 R 复位会把旧对象的值写进新对象

            Refresh();
            if (_enumType != null) OnPropertyChanged(nameof(EnumSelectedItem));

            // 公式条的展开状态跟着新宿主的数据走（构造时也是这个规则）：
            // 不更新会沿用上一项的"展开/收起"，切到空公式条目仍显示覆盖条。
            if (_isFormulaBound) IsFormulaEditorOpen = !string.IsNullOrEmpty(FormulaString);
        }

        /// <summary>浏览文件</summary>
        private void BrowseFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = string.IsNullOrEmpty(FileFilter)
                ? $"{LocalizationManager.GetString("AllFiles")}|*.*"
                : $"{LocalizationManager.GetString("Files")}|{FileFilter}|{LocalizationManager.GetString("AllFiles")}|*.*";
            dlg.Title = LocalizationManager.GetString("SelectFile");
            if (!string.IsNullOrEmpty(ValueString))
            {
                var dir = System.IO.Path.GetDirectoryName(ValueString);
                if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
                    dlg.InitialDirectory = dir;
            }
            if (dlg.ShowDialog() == true)
            {
                Value = dlg.FileName;
            }
        }

        /// <summary>浏览目录（[DirectoryPath] 特性：路径类属性变成「文本框 + 浏览按钮」）。</summary>
        private void BrowseDirectory()
        {
            var attr = GetAttribute<DirectoryPathAttribute>();
            var dlg = new PropertyGridLib.Dialogs.FolderBrowserDialog();

            // 标题/初始目录优先取特性上的声明（自定义文字经 TextResolver 翻译），
            // 没写才回落到本地化文案与当前值
            var title = T(attr?.Title);
            dlg.Description = string.IsNullOrEmpty(title) ? LocalizationManager.GetString("SelectFolder") : title;
            if (!string.IsNullOrEmpty(title)) dlg.Title = title;

            if (!string.IsNullOrEmpty(ValueString) && System.IO.Directory.Exists(ValueString))
            {
                dlg.SelectedPath = ValueString;
            }
            else if (!string.IsNullOrEmpty(attr?.InitialDirectory) && System.IO.Directory.Exists(attr.InitialDirectory))
            {
                dlg.SelectedPath = attr.InitialDirectory;
            }
            
            var owner = Window.GetWindow(System.Windows.Application.Current.MainWindow);
            if (dlg.ShowDialog(owner) == true)
            {
                Value = dlg.SelectedPath;
            }
        }

        /// <summary>编辑集合（ReadOnly 时打开只读查看模式：隐藏功能按钮、仅可浏览）</summary>
        private void EditCollection()
        {
            // 字典类型：使用字典编辑器（支持复杂值类型的嵌套编辑）
            if (Value is IDictionary dictionary)
            {
                var dialog = new Dialogs.DictionaryEditorDialog(dictionary, IsReadOnly);
                dialog.Owner = System.Windows.Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(ValueString));
                    OnPropertyChanged(nameof(IsDefault));
                    OnPropertyChanged(nameof(IsModified));
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                return;
            }

            if (Value is IList collection)
            {
                var elementType = GetElementType(collection.GetType());

                if (IsSimpleType(elementType))
                {
                    var dialog = new Dialogs.SimpleCollectionEditorDialog(collection, IsReadOnly);
                    dialog.Owner = System.Windows.Application.Current.MainWindow;
                    if (dialog.ShowDialog() == true)
                    {
                        OnPropertyChanged(nameof(Value));
                        OnPropertyChanged(nameof(ValueString));
                        OnPropertyChanged(nameof(IsDefault));
                        OnPropertyChanged(nameof(IsModified));
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    var dialog = new Dialogs.CollectionEditorDialog(collection, IsReadOnly);
                    dialog.Owner = System.Windows.Application.Current.MainWindow;
                    if (dialog.ShowDialog() == true)
                    {
                        OnPropertyChanged(nameof(Value));
                        OnPropertyChanged(nameof(ValueString));
                        OnPropertyChanged(nameof(IsDefault));
                        OnPropertyChanged(nameof(IsModified));
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>按钮点击处理：先反射调用宿主方法，再触发 ButtonClicked 事件</summary>
        private void OnButtonClicked()
        {
            InvokeButtonAction();
            ButtonClicked?.Invoke(this, new PropertyButtonClickEventArgs(this, _owner, _propertyInfo.Name));
        }

        /// <summary>通过反射调用 [Button(Action=...)] 指定的宿主方法（支持无参或单参）</summary>
        private void InvokeButtonAction()
        {
            if (string.IsNullOrEmpty(_buttonAction)) return;

            var method = _owner.GetType().GetMethod(_buttonAction,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) return;

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                method.Invoke(_owner, null);
            }
            else if (parameters.Length == 1)
            {
                var ptype = parameters[0].ParameterType;
                object arg = null;
                if (ptype.IsInstanceOfType(this)) arg = this;
                else if (ptype.IsInstanceOfType(_owner)) arg = _owner;
                method.Invoke(_owner, new[] { arg });
            }
        }

        /// <summary>加载子属性</summary>
        private void LoadChildProperties()
        {
            if (Value == null || _currentDepth >= _maxDepth)
            {
                _childProperties = new List<PropertyItem>();
                return;
            }

            // Type 及其子类的值（反射元数据）不展开，避免遍历 Type 属性时反射取值崩溃
            if (Value is Type)
            {
                _childProperties = new List<PropertyItem>();
                return;
            }

            var type = Value.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            _childProperties = new List<PropertyItem>();
            foreach (var prop in properties)
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                var browsableAttr = prop.GetCustomAttribute<BrowsableAttribute>();
                if (browsableAttr != null && !browsableAttr.Browsable) continue;
                // [field:NonSerialized] 也跳过显示
                var bf = prop.DeclaringType?.GetField($"<{prop.Name}>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (bf != null && bf.IsNotSerialized) continue;
                try
                {
                    var item = new PropertyItem(prop, Value, _maxDepth, _currentDepth + 1)
                    {
                        Parent = this,
                        // 网格级只读向下传播：只读父项的子项同样只读
                        IsReadOnly = IsReadOnly
                    };
                    _childProperties.Add(item);
                }
                catch
                {
                    // 单个子属性构造失败（取值/克隆异常）时跳过该子属性，不让整个展开/折叠崩溃
                }
            }
        }

        /// <summary>检测自定义编辑器</summary>
        private void DetectCustomEditor(PropertyInfo propertyInfo)
        {
            // ===== 宿主自定义编辑器（优先级最高，先查宿主注册） =====
            // PropertyGrid.CustomEditors 是静态字典，宿主可以 RegisterEditor<MyAttr>(template)
            // 这里遍历属性的所有 Attribute（走缓存，避免再反射），看看有没有宿主注册过的
            if (PropertyGrid.CustomEditors.Count > 0)
            {
                var attrs = GetAttributes(propertyInfo);
                foreach (var attr in attrs)
                {
                    var attrType = attr.GetType();
                    if (PropertyGrid.CustomEditors.TryGetValue(attrType, out var hostTemplate))
                    {
                        CustomEditorTemplate = hostTemplate;
                        CustomEditorAttribute = attr;
                        if (_isFormulaBound) goto formulaCheck;
                        return;
                    }
                }
            }

            // 检查 Button 特性（命令按钮，无值语义，优先级最高）
            var buttonAttr = GetAttribute<ButtonAttribute>();
            if (buttonAttr != null)
            {
                CustomEditor = CustomEditorType.Button;
                _buttonTextRaw = buttonAttr.Text;
                _buttonAction = buttonAttr.Action;
                // 按钮无“值”语义：get-only 属性不应被误禁用，仅显式 [ReadOnly(true)] 才禁用
                _buttonEnabled = !(GetAttribute<ReadOnlyAttribute>()?.IsReadOnly == true);
                return;
            }

            // FormulaBound<T> 时用内部类型 T 做编辑器检测
            var effectiveType = _isFormulaBound ? _formulaBoundValueType : propertyInfo.PropertyType;

            // 检查 ToggleSwitch 特性（bool 专用开关编辑器；仅对 bool 生效，其它类型忽略该特性）
            var toggleSwitchAttr = GetAttribute<ToggleSwitchAttribute>();
            if (toggleSwitchAttr != null && effectiveType == typeof(bool))
            {
                CustomEditor = CustomEditorType.ToggleSwitch;
                ToggleOnText = toggleSwitchAttr.OnText;
                ToggleOffText = toggleSwitchAttr.OffText;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 检查 MultilineText 特性（string 专用多行文本编辑器；仅对 string 生效）
            var multilineAttr = GetAttribute<MultilineTextAttribute>();
            if (multilineAttr != null && effectiveType == typeof(string))
            {
                CustomEditor = CustomEditorType.MultilineText;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 检查 NumberSlider 特性
            var sliderAttr = GetAttribute<NumberSliderAttribute>();
            if (sliderAttr != null)
            {
                CustomEditor = CustomEditorType.Slider;
                SliderMinimum = sliderAttr.Minimum;
                SliderMaximum = sliderAttr.Maximum;
                SliderStep = sliderAttr.Step;
                SliderShowNumberBox = sliderAttr.ShowNumberBox;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 检查 FilePath 特性
            var filePathAttr = GetAttribute<FilePathAttribute>();
            if (filePathAttr != null)
            {
                CustomEditor = CustomEditorType.FilePath;
                FileFilter = filePathAttr.Filter;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 检查 DirectoryPath 特性
            var dirPathAttr = GetAttribute<DirectoryPathAttribute>();
            if (dirPathAttr != null)
            {
                CustomEditor = CustomEditorType.DirectoryPath;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 检查 CollectionEditor 特性
            var collectionAttr = GetAttribute<CollectionEditorAttribute>();
            if (collectionAttr != null)
            {
                // 字典类型走字典编辑器，其余走集合编辑器
                CustomEditor = IsDictionaryType(effectiveType) ? CustomEditorType.Dictionary : CustomEditorType.Collection;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 检查 MultiSelect 特性（多选列表，动态可选项）
            var multiSelectAttr = GetAttribute<MultiSelectAttribute>();
            if (multiSelectAttr != null)
            {
                CustomEditor = CustomEditorType.MultiSelect;
                if (multiSelectAttr.ProviderType != null)
                {
                    MultiSelectProvider = (IMultiSelectProvider)Activator.CreateInstance(multiSelectAttr.ProviderType);
                }
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 检查 TypeConverter 标准值（下拉列表）
            if (_isFormulaBound)
            {
                // FormulaBound<T> 时检查 T 的 TypeConverter
                var converter = TypeDescriptor.GetConverter(_formulaBoundValueType);
                if (converter != null && converter.GetStandardValuesSupported())
                {
                    var values = converter.GetStandardValues();
                    if (values != null && values.Count > 0)
                    {
                        StandardValues = values.Cast<object>().ToList();
                        IsStandardValuesExclusive = converter.GetStandardValuesExclusive();
                        CustomEditor = CustomEditorType.DropDown;
                        goto formulaCheck;
                    }
                }
            }
            else
            {
                var props = TypeDescriptor.GetProperties(_owner);
                var descriptor = props[propertyInfo.Name];
                if (descriptor != null)
                {
                    var converter = descriptor.Converter;
                    if (converter != null && converter.GetStandardValuesSupported())
                    {
                        var values = converter.GetStandardValues();
                        if (values != null && values.Count > 0)
                        {
                            StandardValues = values.Cast<object>().ToList();
                            IsStandardValuesExclusive = converter.GetStandardValuesExclusive();
                            CustomEditor = CustomEditorType.DropDown;
                            return;
                        }
                    }
                }
            }

            // 自动检测字典类型（IDictionary / IDictionary<,>），须在 ICollection 之前判断
            if (IsDictionaryType(effectiveType))
            {
                CustomEditor = CustomEditorType.Dictionary;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 自动检测集合类型
            if (typeof(IList).IsAssignableFrom(effectiveType) ||
                typeof(ICollection).IsAssignableFrom(effectiveType))
            {
                CustomEditor = CustomEditorType.Collection;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            // 自动检测复杂对象
            // 前置拦截：Color 是 struct，CanExpand 会误判为 Expandable，需在此之前检测
            if (IsColorType(effectiveType))
            {
                CustomEditor = CustomEditorType.Color;
                if (_isFormulaBound) goto formulaCheck;
                return;
            }

            if (CanExpand(effectiveType))
            {
                CustomEditor = CustomEditorType.Expandable;
            }

        formulaCheck:
            // 检查 FormulaEditor 特性
            var formulaAttr = GetAttribute<FormulaEditorAttribute>();
            if (formulaAttr != null)
            {
                IsFormulaEnabled = true;
                if (formulaAttr.ProviderType != null)
                {
                    FormulaTreeProvider = (IFormulaTreeProvider)Activator.CreateInstance(formulaAttr.ProviderType);
                }
            }

            // FormulaBound<T> 即使没有 FormulaEditor 特性也自动启用公式绑定
            if (_isFormulaBound)
            {
                IsFormulaEnabled = true;
            }
        }

        /// <summary>判断类型是否可展开</summary>
        private bool CanExpand(Type type)
        {
            if (_currentDepth >= _maxDepth)
                return false;

            if (IsSimpleType(type))
                return false;

            if (typeof(IList).IsAssignableFrom(type) || typeof(ICollection).IsAssignableFrom(type))
                return false;

            if (type.IsArray)
                return false;

            // Type 及其子类不可展开：Type 的成员（DeclaringMethod 等）是反射元数据，
            // 展开遍历时对其 GetValue 会抛 "只能对 Type.IsGenericParameter 为 True 的类型调用方法"
            // （RuntimeType.get_DeclaringMethod），是 DesignerDemo 切控件卡顿/闪退的根源。
            if (type == typeof(Type) || typeof(Type).IsAssignableFrom(type))
                return false;

            return true;
        }

        /// <summary>判断是否为简单类型</summary>
        private bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsSimpleType(Nullable.GetUnderlyingType(type));
            }

            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(TimeSpan)
                || type == typeof(Guid);
        }

        /// <summary>判断类型是否为字典（实现非泛型 IDictionary 或泛型 IDictionary&lt;,&gt;）</summary>
        private static bool IsDictionaryType(Type type)
        {
            if (type == null) return false;
            if (typeof(IDictionary).IsAssignableFrom(type)) return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)) return true;
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return true;
            }
            return false;
        }

        /// <summary>判断是否为颜色类型（Color / Color? / SolidColorBrush）</summary>
        private static bool IsColorType(Type type)
        {
            if (type == null) return false;
            // 可空类型取底层
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            if (underlying == typeof(Color)) return true;
            if (underlying == typeof(SolidColorBrush)) return true;
            // 不匹配 LinearGradientBrush、RadialGradientBrush 等非纯色画刷
            return false;
        }

        /// <summary>获取集合元素类型</summary>
        private Type GetElementType(Type collectionType)
        {
            if (collectionType.IsGenericType)
            {
                var genericArgs = collectionType.GetGenericArguments();
                if (genericArgs.Length > 0)
                    return genericArgs[0];
            }

            if (collectionType.IsArray)
                return collectionType.GetElementType();

            return typeof(object);
        }

        /// <summary>检查属性的后备字段是否标记了 [NonSerialized]</summary>
        private bool IsNonSerializedProperty()
        {
            var declaringType = _propertyInfo.DeclaringType;
            if (declaringType == null) return false;
            var backingField = declaringType.GetField($"<{_propertyInfo.Name}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return backingField?.IsNotSerialized ?? false;
        }

        /// <summary>深拷贝对象</summary>
        private static object DeepClone(object obj)
        {
            return PropertyGridLib.Helpers.DeepCloneHelper.DeepClone(obj);
        }

        /// <summary>获取特性</summary>
        /// <summary>
        /// 特性反射缓存：按 PropertyInfo 缓存全部特性数组。
        /// 旧实现每个 PropertyItem 构造要调 12+ 次 Attribute.GetCustomAttribute（每次都重新反射元数据），
        /// 对象属性上百个时累计几千次反射 —— 这是“每个属性项实例化开销大”的主因。
        /// 缓存后每个属性只反射一次，特性对象只读（属性值不可变），共享安全。
        /// </summary>
        private static readonly ConcurrentDictionary<PropertyInfo, Attribute[]> AttributeCache =
            new ConcurrentDictionary<PropertyInfo, Attribute[]>();

        private static Attribute[] GetAttributes(PropertyInfo propertyInfo)
        {
            return AttributeCache.GetOrAdd(propertyInfo,
                p => p.GetCustomAttributes(true).OfType<Attribute>().ToArray());
        }

        private T GetAttribute<T>() where T : Attribute
        {
            var attrs = GetAttributes(_propertyInfo);
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is T t) return t;
            }
            return null;
        }

        /// <summary>应用全局翻译钩子（LocalizationManager.TextResolver），未设置时返回原文</summary>
        private static string T(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var resolver = LocalizationManager.TextResolver;
            return resolver != null ? resolver(text) : text;
        }

        /// <summary>更新本地化名称、描述和分类（检查 IPropertyLocalization 接口）</summary>
        private void UpdateLocalizedNames()
        {
            // 首次调用记下基准名（特性原文）：排序永远用基准名，语言切换不影响顺序
            _baseDisplayName ??= GetAttribute<DisplayNameAttribute>()?.DisplayName ?? _propertyInfo.Name;
            _baseCategory ??= GetAttribute<CategoryAttribute>()?.Category ?? _propertyInfo.Name;
            // 取词链（四级）：模型接口动态文本 → 特性原文经 TextResolver 翻译 → 宿主注册的属性名翻译表 → 原样兜底
            var localizable = _owner as IPropertyLocalization;

            if (localizable != null)
            {
                // 模型自带本地化实现：其返回值已是最终译文，不再过全局钩子；
                // 仅当它返回 null 回退到特性原文时才翻译。
                var name = localizable.GetDisplayName(_propertyInfo.Name);
                DisplayName = name ?? T(GetAttribute<DisplayNameAttribute>()?.DisplayName
                    ?? LocalizationManager.TryGetPropertyName(_propertyInfo.Name)
                    ?? _propertyInfo.Name);

                var desc = localizable.GetDescription(_propertyInfo.Name);
                Description = desc ?? T(GetAttribute<DescriptionAttribute>()?.Description ?? string.Empty);

                var cat = localizable.GetCategory(_propertyInfo.Name);
                Category = cat ?? T(GetAttribute<CategoryAttribute>()?.Category ?? LocalizationManager.GetString("Misc"));
            }
            else
            {
                DisplayName = T(GetAttribute<DisplayNameAttribute>()?.DisplayName
                    ?? LocalizationManager.TryGetPropertyName(_propertyInfo.Name)
                    ?? _propertyInfo.Name);
                Description = T(GetAttribute<DescriptionAttribute>()?.Description ?? string.Empty);
                Category = T(GetAttribute<CategoryAttribute>()?.Category ?? LocalizationManager.GetString("Misc"));
            }
        }

        /// <summary>更新本地化（供 PropertyGrid 在语言切换时调用，递归更新子属性）</summary>
        public void UpdateLocalization()
        {
            UpdateLocalizedNames();
            OnPropertyChanged(nameof(ButtonText));
            // 开关状态文字：null 文本走库内置词典（开/ON、关/OFF），自定义文本走 TextResolver，都随语言变化
            OnPropertyChanged(nameof(ToggleStateText));
            OnPropertyChanged(nameof(ToggleStateTextVisibility));
            if (_enumType != null)
            {
                BuildEnumDisplayItems();   // 枚举显示文本随语言刷新
                OnPropertyChanged(nameof(EnumDisplayItems));
                OnPropertyChanged(nameof(EnumSelectedItem));
            }
            if (_childProperties != null)
            {
                foreach (var child in _childProperties)
                {
                    child.UpdateLocalization();
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Value 变化时同步刷新 [ToggleSwitch] 编辑器的状态文字（开/关文字随值切换）
            if (propertyName == nameof(Value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToggleStateText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToggleStateTextVisibility)));
            }
            else if (propertyName == nameof(IsExpanded))
            {
                // 嵌套子项的展开/折叠必须逐级冒泡到顶层项：
                // 网格（PropertyGrid / PropertyGridPro）只对顶层项挂钩 PropertyChanged
                // （刷新循环里仅遍历 _allProperties），惰性加载出来的子项无人订阅。
                // 子项自身 IsExpanded 变化若不冒泡，扁平行集合（BuildFlatRows）就不会重建，
                // 现象即「第一层能展开、第二层及更深层点箭头无任何反应」。
                // 冒泡到顶层项后由网格统一重建一次行集合；顶层项 Parent == null，链条自然终止。
                Parent?.OnPropertyChanged(nameof(IsExpanded));
            }
        }

        #endregion
    }

    /// <summary>
    /// 简单的 RelayCommand 实现。
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
