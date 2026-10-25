using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PropertyGridLib.Controls;
using PropertyGridLib.Localization;

namespace PropertyGridLib
{
    /// <summary>
    /// WPF PropertyGrid 控件 —— 类似 WinForms PropertyGrid 的完整实现。
    /// 支持特性：[Browsable], [Category], [Description], [ReadOnly], [DefaultValue], [DisplayName]
    /// 支持编辑器：string, bool, int, double, float, decimal, enum, Color, DateTime, 可展开对象
    /// </summary>
    public class PropertyGrid : Control
    {
        #region 依赖属性
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid),
                new PropertyMetadata(null, OnSelectedObjectChanged));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(PropertyGrid),
                new PropertyMetadata(string.Empty, OnSearchTextChanged));

        public static readonly DependencyProperty ShowDescriptionProperty =
            DependencyProperty.Register(nameof(ShowDescription), typeof(bool), typeof(PropertyGrid),
                // 默认开启：底部常驻描述区（可上下拖动分隔条调整高度）。
                // 不想占高度的宿主可显式关掉（描述栏 + 拖拽分割条一起隐藏）。
                new PropertyMetadata(true, OnShowDescriptionChanged));

        public static readonly DependencyProperty ShowSearchBarProperty =
            DependencyProperty.Register(nameof(ShowSearchBar), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(true));

        /// <summary>是否显示「展开 | 折叠」批量按钮组：宿主只想要纯属性表时可关掉。</summary>
        public static readonly DependencyProperty ShowExpandCollapseProperty =
            DependencyProperty.Register(nameof(ShowExpandCollapse), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(true));

        /// <summary>是否显示分类头：false = 不分组，所有属性按原顺序平铺往下排。</summary>
        public static readonly DependencyProperty ShowCategoryHeadersProperty =
            DependencyProperty.Register(nameof(ShowCategoryHeaders), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(true, OnShowCategoryHeadersChanged));

        /// <summary>
        /// 是否显示顶部标题区（如「属性」）。false 时整块收起；与 Header 空值规则叠加
        /// （Header 为空本来就不占高度）。与 PropertyGridPro.ShowHeader 同名同语义。
        /// </summary>
        public static readonly DependencyProperty ShowHeaderProperty =
            DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(true, OnShowHeaderChanged));

        /// <summary>是否显示顶部标题区（默认显示）。</summary>
        public bool ShowHeader
        {
            get => (bool)GetValue(ShowHeaderProperty);
            set => SetValue(ShowHeaderProperty, value);
        }

        /// <summary>
        /// 顶部标题（如「属性」）。为空时不占任何高度 ——
        /// 宿主把它当纯控件用时（嵌在别处的面板里）不需要额外标题。
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(PropertyGrid),
                // 默认就叫「属性」：标题就是"属性"这件事本身的默认值，
                // 宿主有特殊语境（如「项目属性」「参数」）再覆盖；设成空串可完全隐藏。
                new PropertyMetadata("属性", OnHeaderChanged));

        public static readonly DependencyProperty SelectedPropertyItemProperty =
            DependencyProperty.Register(nameof(SelectedPropertyItem), typeof(IPropertyItem), typeof(PropertyGrid),
                new PropertyMetadata(null, OnSelectedPropertyItemChanged));

        public static readonly DependencyProperty GroupedCategoriesProperty =
            DependencyProperty.Register(nameof(GroupedCategories), typeof(IEnumerable), typeof(PropertyGrid),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(false));

        public static readonly DependencyProperty FormulaTreeProviderProperty =
            DependencyProperty.Register(nameof(FormulaTreeProvider), typeof(IFormulaTreeProvider), typeof(PropertyGrid),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsAllExpandedProperty =
            DependencyProperty.Register(nameof(IsAllExpanded), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(true, OnIsAllExpandedChanged));

        public static readonly DependencyProperty ShowTypeHintProperty =
            DependencyProperty.Register(nameof(ShowTypeHint), typeof(bool), typeof(PropertyGrid),
                // 默认开启：绑定公式后输入框旁直接标注 "(int)" 这类目标类型，选绑定源时一眼对上号。
                // 嫌挤的宿主可显式设为 false。
                new PropertyMetadata(true));

        // ===== 网格级只读：所有条目（含懒加载的子属性）强制只读，编辑器变只读、重置禁用 =====

        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register(nameof(ReadOnly), typeof(bool), typeof(PropertyGrid),
                new FrameworkPropertyMetadata(false, OnReadOnlyChanged));

        // ===== 1.4.0 新增：属性列表 UI 虚拟化（默认开启；宿主遇到滚动串值可关闭回退） =====

        public static readonly DependencyProperty EnableVirtualizationProperty =
            DependencyProperty.Register(nameof(EnableVirtualization), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(true, OnEnableVirtualizationChanged));

        // ===== 1.4.0 新增：标题位置（Small 紧凑布局：标题在左与编辑器平齐） =====

        public static readonly DependencyProperty TitlePlacementProperty =
            DependencyProperty.Register(nameof(TitlePlacement), typeof(PropertyGridTitlePlacement), typeof(PropertyGrid),
                new PropertyMetadata(PropertyGridTitlePlacement.Top, OnTitlePlacementChanged));

        // ===== 1.4.0 新增：标题/分类头字体粗细（默认 Bold，可配 Normal/Light 等） =====

        public static readonly DependencyProperty TitleFontWeightProperty =
            DependencyProperty.Register(nameof(TitleFontWeight), typeof(FontWeight), typeof(PropertyGrid),
                new PropertyMetadata(FontWeights.Bold));

        public static readonly DependencyProperty CategoryFontWeightProperty =
            DependencyProperty.Register(nameof(CategoryFontWeight), typeof(FontWeight), typeof(PropertyGrid),
                new PropertyMetadata(FontWeights.Bold));

        // ===== 1.4.0 新增：事件页支持（默认全部关闭，不影响既有宿主） =====

        public static readonly DependencyProperty ShowEventsPageProperty =
            DependencyProperty.Register(nameof(ShowEventsPage), typeof(bool), typeof(PropertyGrid),
                new PropertyMetadata(false, OnShowEventsPageChanged));

        public static readonly DependencyProperty ActiveTabProperty =
            DependencyProperty.Register(nameof(ActiveTab), typeof(PropertyGridTab), typeof(PropertyGrid),
                new PropertyMetadata(PropertyGridTab.Properties, OnActiveTabChanged));

        public static readonly DependencyProperty EventsProperty =
            DependencyProperty.Register(nameof(Events), typeof(System.Collections.Generic.IList<Controls.EventItem>), typeof(PropertyGrid),
                new PropertyMetadata(null, OnEventsChanged));

        public static readonly DependencyProperty GroupedEventCategoriesProperty =
            DependencyProperty.Register(nameof(GroupedEventCategories), typeof(IEnumerable), typeof(PropertyGrid),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EventEditRequestedCommandProperty =
            DependencyProperty.Register(nameof(EventEditRequestedCommand), typeof(System.Windows.Input.ICommand), typeof(PropertyGrid),
                new PropertyMetadata(null));

        /// <summary>绑定的目标对象</summary>
        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        /// <summary>搜索文本</summary>
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        /// <summary>是否显示底部描述栏</summary>
        public bool ShowDescription
        {
            get => (bool)GetValue(ShowDescriptionProperty);
            set => SetValue(ShowDescriptionProperty, value);
        }

        /// <summary>是否显示搜索栏</summary>
        public bool ShowSearchBar
        {
            get => (bool)GetValue(ShowSearchBarProperty);
            set => SetValue(ShowSearchBarProperty, value);
        }

        /// <summary>是否显示「展开 | 折叠」批量按钮组（默认显示）</summary>
        public bool ShowExpandCollapse
        {
            get => (bool)GetValue(ShowExpandCollapseProperty);
            set => SetValue(ShowExpandCollapseProperty, value);
        }

        /// <summary>
        /// 是否显示分类头（默认显示）。设为 false 时不做分组：所有属性按声明顺序平铺往下排，
        /// 没有分类标题与折叠箭头。
        /// </summary>
        public bool ShowCategoryHeaders
        {
            get => (bool)GetValue(ShowCategoryHeadersProperty);
            set => SetValue(ShowCategoryHeadersProperty, value);
        }

        /// <summary>顶部标题（如「属性」）；留空则不占高度。</summary>
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>当前选中的属性项</summary>
        public IPropertyItem SelectedPropertyItem
        {
            get => (IPropertyItem)GetValue(SelectedPropertyItemProperty);
            set => SetValue(SelectedPropertyItemProperty, value);
        }

        /// <summary>分组后的分类列表（供 XAML 绑定）</summary>
        public IEnumerable GroupedCategories
        {
            get => (IEnumerable)GetValue(GroupedCategoriesProperty);
            set => SetValue(GroupedCategoriesProperty, value);
        }

        /// <summary>是否正在加载（切换 SelectedObject 时显示动画）</summary>
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        /// <summary>公式绑定树数据提供者（由宿主应用设置）</summary>
        public IFormulaTreeProvider FormulaTreeProvider
        {
            get => (IFormulaTreeProvider)GetValue(FormulaTreeProviderProperty);
            set => SetValue(FormulaTreeProviderProperty, value);
        }

        /// <summary>是否展开所有分类和可展开属性(顶部"展开/折叠"按钮绑定此属性)</summary>
        public bool IsAllExpanded
        {
            get => (bool)GetValue(IsAllExpandedProperty);
            set => SetValue(IsAllExpandedProperty, value);
        }

        /// <summary>是否显示类型提示(如 "(int)"、"(string)")。
        /// 影响两个位置:1)公式绑定区右侧的类型提示;2)公式绑定弹窗中每条树节点后方的类型提示。
        /// 默认 true(显示)。
        /// </summary>
        public bool ShowTypeHint
        {
            get => (bool)GetValue(ShowTypeHintProperty);
            set => SetValue(ShowTypeHintProperty, value);
        }

        /// <summary>
        /// 属性列表是否启用 UI 虚拟化（默认 true）。
        /// 开启时只生成视口内的属性行并复用容器，切换选中/刷新的开销从「全量重建上百行」降到十几行
        /// （实测选中重建 206ms → 8ms）。代价是行容器会被回收复用（Recycling）。
        /// 若宿主自定义行模板出现「滚动后行内值串行」，设为 false 即回到 1.3.4 的全量生成行为。
        /// </summary>
        public bool EnableVirtualization
        {
            get => (bool)GetValue(EnableVirtualizationProperty);
            set => SetValue(EnableVirtualizationProperty, value);
        }

        /// <summary>
        /// 网格级只读：true 时所有条目（含懒加载的子属性）强制只读 ——
        /// 编辑器按各自模板变只读（文本框只读 / 下拉与滑块禁用 / 重置按钮禁用）。
        /// 典型用途：集合编辑器只读模式下内嵌的属性页。切换后重建属性列表生效。
        /// </summary>
        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }

        private static void OnReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyGrid)d).RefreshProperties();
        }

        /// <summary>
        /// 属性行标题排布方式。默认 Top（标题在编辑器上方）；设为 Left 时标题与编辑器平齐一行，
        /// 走 Small / 紧凑布局，与 HandyControl 表单左标题风格一致。
        /// </summary>
        public PropertyGridTitlePlacement TitlePlacement
        {
            get => (PropertyGridTitlePlacement)GetValue(TitlePlacementProperty);
            set => SetValue(TitlePlacementProperty, value);
        }

        /// <summary>
        /// 属性行标题字体粗细（默认 Bold）。宿主可设为 Normal / Light 等以匹配自身设计风格。
        /// 该属性通过绑定直接生效，无需刷新属性列表。
        /// </summary>
        public FontWeight TitleFontWeight
        {
            get => (FontWeight)GetValue(TitleFontWeightProperty);
            set => SetValue(TitleFontWeightProperty, value);
        }

        /// <summary>
        /// 分类头（Category / Expander 标题）字体粗细（默认 Bold）。
        /// </summary>
        public FontWeight CategoryFontWeight
        {
            get => (FontWeight)GetValue(CategoryFontWeightProperty);
            set => SetValue(CategoryFontWeightProperty, value);
        }

        /// <summary>
        /// 是否启用事件页（属性/事件双页切换）。默认 false：控件行为与旧版完全一致，不影响既有宿主。
        /// 设为 true 后：
        /// - 顶部出现「属性 | 事件」切换按钮（默认显示属性页）；
        /// - 事件页展示 <see cref="Events"/> 注入的事件列表（未注入时自动反射 <see cref="SelectedObject"/> 的公共事件）；
        /// - 双击事件行或点 ⚡ 触发 <see cref="EventEditRequested"/>（宿主接代码编辑器，如 CodeForge）。
        /// </summary>
        public bool ShowEventsPage
        {
            get => (bool)GetValue(ShowEventsPageProperty);
            set => SetValue(ShowEventsPageProperty, value);
        }

        /// <summary>当前激活页（Properties=属性页 / Events=事件页；仅 ShowEventsPage=true 时可切到 Events）</summary>
        public PropertyGridTab ActiveTab
        {
            get => (PropertyGridTab)GetValue(ActiveTabProperty);
            set => SetValue(ActiveTabProperty, value);
        }

        /// <summary>
        /// 事件数据源（宿主自定义 EventItem 列表，如设计器把 WpfDesign 的 DesignItemProperty 包装进来）。
        /// 为 null 且启用事件页时，自动反射 SelectedObject 的公共事件。
        /// </summary>
        public System.Collections.Generic.IList<Controls.EventItem> Events
        {
            get => (System.Collections.Generic.IList<Controls.EventItem>)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        /// <summary>事件页分组结果（供模板绑定）</summary>
        public IEnumerable GroupedEventCategories
        {
            get => (IEnumerable)GetValue(GroupedEventCategoriesProperty);
            set => SetValue(GroupedEventCategoriesProperty, value);
        }

        /// <summary>
        /// 「编辑事件代码」命令（双击事件行/点⚡时执行，参数为对应 <see cref="Controls.EventItem"/>）。
        /// 与 <see cref="EventEditRequested"/> 事件等价，任选其一接管；两者都未设置时执行内置默认行为（填建议方法名）。
        /// </summary>
        public System.Windows.Input.ICommand EventEditRequestedCommand
        {
            get => (System.Windows.Input.ICommand)GetValue(EventEditRequestedCommandProperty);
            set => SetValue(EventEditRequestedCommandProperty, value);
        }

        /// <summary>请求编辑事件代码（双击事件行或点击 ⚡ 时触发；宿主应打开代码编辑器）</summary>
        public event EventHandler<Controls.EventItemEventArgs> EventEditRequested;

        /// <summary>
        /// 属性值被用户修改后触发（1.4.0 新增）：参数携带属性名与新值。
        /// 设计器宿主（如 WpfDesign 集成）可订阅此事件把修改写回自己的模型；
        /// 普通宿主不受影响（不订阅即无行为变化）。
        /// 注意：嵌套子属性的变更也走此事件，PropertyName 为子属性自身名称。
        /// </summary>
        public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

        #endregion

        #region 字段

        private ItemsControl _itemsControl;
        private List<object> _flatRows;
        private int _flatRowCount;
        private TextBlock _descriptionText;
        private TextBlock _descriptionTitle;
        private FrameworkElement _descriptionBar;
        private FrameworkElement _descriptionSplitter;
        private FrameworkElement _headerTitle;
        private RowDefinition _descriptionRow;      // 描述栏行（固定 80 可拖）：隐藏时压 0，否则内容收了行高不收、底部留空白
        private GridLength _descRowSavedHeight;
        private double _descRowSavedMin;
        private double _descRowSavedMax;
        private bool _descRowCollapsed;
        private TextBox _searchBox;
        private List<IPropertyItem> _allProperties;
        private IPropertyItem _selectedProperty;
        private ContentControl _collectionEditorHost;
        private FrameworkElement _normalHost;

        // 刷新代数：快速连选时旧 BeginInvoke 回调据此作废（见 RefreshProperties）
        private int _refreshGeneration;

        // 上一轮刷新过的对象与搜索条件：换对象时据此判断能否走"原地换绑"快速路径
        // （同类型 + 同搜索条件 = 属性集合与行集合都不变，见 TryRebindSameShape）
        private object _lastRefreshedObject;
        private string _lastFilterText;

        // ===== 1.4.0 事件页部件 =====
        private EventPicker _eventPicker;
        private Border _eventHost;
        private FrameworkElement _tabHost;
        private ToggleButton _propertiesTabButton;
        private ToggleButton _eventsTabButton;

        /// <summary>请求编辑事件代码：由模板内 EventPicker 冒泡而来。
        /// 统一转为 <see cref="EventEditRequested"/> / <see cref="EventEditRequestedCommand"/>；
        /// 两者都未接管时执行默认行为（为未绑定事件填建议方法名）。</summary>
        private void OnEventPickerEditRequested(object sender, EventItemEventArgs e)
        {
            var item = e.Item;
            if (EventEditRequestedCommand != null && EventEditRequestedCommand.CanExecute(item))
            {
                EventEditRequestedCommand.Execute(item);
                return;
            }

            var h = EventEditRequested;
            if (h != null)
            {
                h(this, e);
                return;
            }

            // 无宿主接管 → 默认行为：未绑定时填建议方法名（不改已绑定的）
            if (!item.HasHandler && !item.IsReadOnly)
                item.HandlerName = "On" + item.Name;
        }

        #endregion

        /// <summary>当任意 [Button] 属性按钮被点击时触发（宿主可订阅以统一响应按钮动作）</summary>
        public event EventHandler<PropertyButtonClickEventArgs> ButtonClicked;

        /// <summary>
        /// 全局按钮点击事件：任何 PropertyGrid 实例（包括集合/字典编辑器内嵌的网格、弹窗内的网格）
        /// 触发 [Button] 时都会引发。宿主订阅一次即可统一响应所有层级的按钮，无需逐个网格订阅。
        /// 注意：这是静态事件，宿主窗口关闭时应取消订阅，避免实例被静态引用而无法回收。
        /// </summary>
        public static event EventHandler<PropertyButtonClickEventArgs> GlobalButtonClicked;

        /// <summary>
        /// 转发全局按钮点击事件。C# 事件只能在声明类型内触发，
        /// PropertyGridPro 等扩展面板借用此入口转发自己的 [Button] 点击。
        /// </summary>
        internal static void RaiseGlobalButtonClicked(object sender, PropertyButtonClickEventArgs e)
            => GlobalButtonClicked?.Invoke(sender, e);

        public static FontFamily GlobalFontFamily { get; set; } = new FontFamily("Consolas");
        public static double GlobalFontSize { get; set; } = 12;

        /// <summary>
        /// 宿主自定义编辑器注册：Attribute 类型 → 编辑器 DataTemplate。
        /// 宿主在 PropertyGrid 实例化前注册，PropertyItem.DetectCustomEditor 会优先查此字典。
        /// 用法示例：
        ///   PropertyGrid.CustomEditors[typeof(MyUnitEditorAttribute)] = myUnitTemplate;
        /// </summary>
        public static Dictionary<Type, DataTemplate> CustomEditors { get; } = new Dictionary<Type, DataTemplate>();

        /// <summary>
        /// 注册自定义编辑器：给定 Attribute 类型和对应的编辑器 DataTemplate。
        /// PropertyGrid 扫描属性时，如果发现属性带此 Attribute，就用该 DataTemplate 渲染。
        /// </summary>
        public static void RegisterEditor<TAttribute>(DataTemplate template) where TAttribute : Attribute
        {
            CustomEditors[typeof(TAttribute)] = template;
        }

        #region 构造
        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid),
                new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }

        public PropertyGrid()
        {
            _allProperties = new List<IPropertyItem>();
            this.PreviewMouseLeftButtonDown += OnPropertyRowClicked;
            LocalizationManager.LanguageChanged += OnLanguageChanged;

            // 监听 FontSize / FontFamily 变化，实时同步到静态全局值 —— 二级 Dialog（字典编辑器、
            // 集合编辑器等）从 GlobalFontSize/GlobalFontFamily 读取字体，这样宿主修改 PropertyGrid
            // 字体后，后续打开的 Dialog 立即使用新字体，不用等到下一次 OnApplyTemplate。
            var fontSizeDesc = DependencyPropertyDescriptor.FromProperty(FontSizeProperty, typeof(PropertyGrid));
            fontSizeDesc?.AddValueChanged(this, (s, e) =>
            {
                if (FontSize > 0 && Math.Abs(FontSize - GlobalFontSize) > 0.001)
                    GlobalFontSize = FontSize;
            });
            var fontFamilyDesc = DependencyPropertyDescriptor.FromProperty(FontFamilyProperty, typeof(PropertyGrid));
            fontFamilyDesc?.AddValueChanged(this, (s, e) =>
            {
                if (FontFamily != null && !FontFamily.Equals(GlobalFontFamily))
                    GlobalFontFamily = FontFamily;
            });
        }

        private void OnPropertyRowClicked(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            while (source != null && source != this)
            {
                if (source is FrameworkElement fe && fe.DataContext is IPropertyItem item)
                {
                    SelectedPropertyItem = item as PropertyItem;
                    return;
                }
                source = VisualTreeHelper.GetParent(source);
            }
        }

        #endregion

        #region 模板

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 把本实例"实际生效"的字体发布为全局字体（以最后一次渲染的网格为准，同值重复赋值幂等）。
            // 动机：宿主常按 XAML 实例属性设置字体（如 FontSize="15"），不会去设静态 Global*；
            // 而 CollectionEditorDialog 等二级窗口从 Global* 取值 —— 若无人发布，它们拿到的
            // 只是出厂默认值（Consolas/12），与主网格不一致。此处发布后，任何时刻打开的
            // 集合编辑器都与最近使用过的 PropertyGrid 保持同一观感。
            var fam = FontFamily;
            var size = FontSize;
            if (fam != null && !fam.Equals(GlobalFontFamily)) GlobalFontFamily = fam;
            if (size > 0 && Math.Abs(size - GlobalFontSize) > 0.001) GlobalFontSize = size;

            _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
            _descriptionText = GetTemplateChild("PART_DescriptionText") as TextBlock;
            _descriptionTitle = GetTemplateChild("PART_DescriptionTitle") as TextBlock;
            _descriptionBar = GetTemplateChild("PART_DescriptionBar") as FrameworkElement;
            _descriptionSplitter = GetTemplateChild("PART_DescriptionSplitter") as FrameworkElement;
            _headerTitle = GetTemplateChild("PART_HeaderTitle") as FrameworkElement;
            _descriptionRow = GetTemplateChild("PART_DescriptionRow") as RowDefinition;
            ApplyHeader();
            ApplyDescriptionBar();
            _searchBox = GetTemplateChild("PART_SearchBox") as TextBox;
            _collectionEditorHost = GetTemplateChild("PART_CollectionEditorHost") as ContentControl;
            _normalHost = GetTemplateChild("PART_NormalHost") as FrameworkElement;
            ApplyDescriptionBar();

            // 1.4.0：事件页部件（模板未提供时不影响旧模板/旧宿主）
            _eventPicker = GetTemplateChild("PART_EventPicker") as EventPicker;
            _eventHost = GetTemplateChild("PART_EventHost") as Border;
            _tabHost = GetTemplateChild("PART_TabHost") as FrameworkElement;
            _propertiesTabButton = GetTemplateChild("PART_PropertiesTab") as ToggleButton;
            _eventsTabButton = GetTemplateChild("PART_EventsTab") as ToggleButton;

            if (_propertiesTabButton != null)
            {
                _propertiesTabButton.Click -= OnTabButtonClicked;
                _propertiesTabButton.Click += OnTabButtonClicked;
            }
            if (_eventsTabButton != null)
            {
                _eventsTabButton.Click -= OnTabButtonClicked;
                _eventsTabButton.Click += OnTabButtonClicked;
            }
            if (_eventPicker != null)
            {
                _eventPicker.EditRequested -= OnEventPickerEditRequested;
                _eventPicker.EditRequested += OnEventPickerEditRequested;
            }
            PushEventsToPicker();
            UpdateTabVisualState();

            if (_searchBox != null)
            {
                _searchBox.TextChanged += SearchBox_TextChanged;
            }

            // 展开/折叠按钮：通过 Click 事件驱动（不依赖 RadioButton 的 IsChecked 绑定，
            // 避免与 hc:ButtonGroup 内部的选中机制冲突）
            var expandRadio = GetTemplateChild("PART_ExpandRadio") as RadioButton;
            var collapseRadio = GetTemplateChild("PART_CollapseRadio") as RadioButton;
            if (expandRadio != null)
            {
                expandRadio.Click += (s, e) => IsAllExpanded = true;
                expandRadio.IsChecked = IsAllExpanded;
            }
            if (collapseRadio != null)
            {
                collapseRadio.Click += (s, e) => IsAllExpanded = false;
            }

            ApplyVirtualizationMode();

            RefreshProperties();
        }

        #endregion

        #region 属性变更回调
        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (PropertyGrid)d;
            grid.RefreshProperties();
            grid.PushEventsToPicker();
        }

        private static void OnEnableVirtualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyGrid)d).ApplyVirtualizationMode();
        }

        private static void OnTitlePlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TitlePlacement 影响的是行模板选择。刷新 ItemsSource 后，
            // 隐式 DataTemplate 内部的 DataTrigger 会重新评估并切换到对应模板。
            ((PropertyGrid)d).RefreshProperties();
        }

        private static void OnShowCategoryHeadersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (PropertyGrid)d;
            // 显示/隐藏分类头改变的是"行集合结构"，同类型快速刷新会直接复用旧行集合，
            // 所以先让快速路径失效，强制走一次全量重建。
            grid._lastRefreshedObject = null;
            grid.RefreshProperties();
        }

        /// <summary>
        /// 把 EnableVirtualization 落到实际列表控件上。关闭时容器不再回收复用，
        /// 行为回到 1.3.4（全量生成行），用于宿主自定义行模板出现串值时的回退。
        /// 只切 IsVirtualizing —— VirtualizationMode 在面板 Measure 过后再改会抛
        /// InvalidOperationException（WPF 限制），回收模式由模板 XAML 里的
        /// VirtualizationMode="Recycling" 在模板应用时一次性设定，代码运行期不碰。
        /// </summary>
        private void ApplyVirtualizationMode()
        {
            if (_itemsControl == null) return;
            VirtualizingPanel.SetIsVirtualizing(_itemsControl, EnableVirtualization);
        }

        private static void OnShowEventsPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (PropertyGrid)d;
            if (!(bool)e.NewValue)
                grid.ActiveTab = PropertyGridTab.Properties;
            grid.UpdateTabVisualState();
        }

        private static void OnActiveTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (PropertyGrid)d;
            grid.UpdateTabVisualState();
            // 切到事件页时补一次事件构建（此前属性页切对象已跳过懒构建，数据需在此刷新）
            if ((PropertyGridTab)e.NewValue == PropertyGridTab.Events)
                grid.PushEventsToPicker(force: true);
        }

        private static void OnEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyGrid)d).PushEventsToPicker();
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (PropertyGrid)d;
            grid.ApplyFilter();
        }

        private static void OnSelectedPropertyItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (PropertyGrid)d;
            grid.UpdateDescriptionBar();
        }

        private static void OnIsAllExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (PropertyGrid)d;
            if ((bool)e.NewValue)
                grid.ExpandAll();
            else
                grid.CollapseAll();
        }

        private void UpdateDescriptionBar()
        {
            if (_descriptionText == null || _descriptionTitle == null) return;

            var item = SelectedPropertyItem;
            if (item != null && !string.IsNullOrEmpty(item.Description))
            {
                _descriptionTitle.Text = item.DisplayName;
                _descriptionText.Text = item.Description;
                _descriptionText.Visibility = Visibility.Visible;
                _descriptionTitle.Visibility = Visibility.Visible;
            }
            else
            {
                _descriptionText.Visibility = Visibility.Collapsed;
                _descriptionTitle.Visibility = Visibility.Collapsed;
            }

            ApplyDescriptionBar();
        }

        /// <summary>
        /// ShowDescription 变化 → 立即重算描述栏/分割条显隐。
        /// 打开时若没有选中项，先走一遍 ApplyFilter —— 它内部的"自动选中第一个属性行"
        /// 只在 ShowDescription=true 时生效；勾选回调不会刷新列表，不补这一步就会出现
        /// "勾了显示描述栏、栏是空的"（demo 实测：先关再开普通版不显示）。
        /// </summary>
        private static void OnShowDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyGrid grid)
            {
                if ((bool)e.NewValue && grid.SelectedPropertyItem == null)
                    grid.ApplyFilter();
                grid.ApplyDescriptionBar();
            }
        }

        private static void OnShowHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyGrid grid) grid.ApplyHeader();
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyGrid grid) grid.ApplyHeader();
        }

        /// <summary>
        /// 顶部标题区显隐 = ShowHeader && Header 非空（cs 接管，每次状态变化重算本地值，
        /// 覆盖模板里"Header 空则隐藏"的触发器——两者语义叠加而非竞争）。
        /// </summary>
        private void ApplyHeader()
        {
            if (_headerTitle == null) return;
            _headerTitle.Visibility = ShowHeader && !string.IsNullOrEmpty(Header)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// 底部描述栏 + 拖拽分割条的显隐总开关：
        /// ShowDescription=false 时整块收起（连分割条一起），并把<b>固定高度的描述行压到 0</b> ——
        /// RowDefinition Height="80" 不随内容收缩，只 Collapsed 内容会底部留一条空白；
        /// true 时恢复记忆的行高（首次隐藏前记下，之后显示/隐藏来回切不丢用户拖拽过的尺寸）。
        /// 直接写本地值覆盖模板绑定 —— 每次状态变化都会重算，不依赖绑定。
        /// </summary>
        private void ApplyDescriptionBar()
        {
            var vis = ShowDescription && SelectedPropertyItem != null
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (_descriptionBar != null) _descriptionBar.Visibility = vis;
            if (_descriptionSplitter != null) _descriptionSplitter.Visibility = vis;

            if (_descriptionRow == null) return;
            if (vis == Visibility.Collapsed)
            {
                if (!_descRowCollapsed)
                {
                    _descRowSavedHeight = _descriptionRow.Height;
                    _descRowSavedMin = _descriptionRow.MinHeight;
                    _descRowSavedMax = _descriptionRow.MaxHeight;
                    _descRowCollapsed = true;
                }
                _descriptionRow.Height = new GridLength(0);
                _descriptionRow.MinHeight = 0;
                _descriptionRow.MaxHeight = 0;
            }
            else if (_descRowCollapsed)
            {
                _descriptionRow.Height = _descRowSavedHeight;
                _descriptionRow.MinHeight = _descRowSavedMin;
                _descriptionRow.MaxHeight = _descRowSavedMax;
                _descRowCollapsed = false;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchBox != null)
                SearchText = _searchBox.Text;
        }

        #endregion

        #region 事件页逻辑（1.4.0 新增，默认关闭）

        private void OnTabButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, _eventsTabButton))
                ActiveTab = PropertyGridTab.Events;
            else if (ReferenceEquals(sender, _propertiesTabButton))
                ActiveTab = PropertyGridTab.Properties;
        }

        /// <summary>把事件数据推给模板中的 EventPicker（未显式注入 Events 时自动反射 SelectedObject）。
        /// 懒构建：事件页未激活时跳过（避免每次切对象都反射构建全部事件 → 进过事件页后切对象变慢）；
        /// force=true 由"切到事件页"触发，确保数据新鲜。</summary>
        internal void PushEventsToPicker(bool force = false)
        {
            if (_eventPicker == null) return;
            if (!force && ActiveTab != PropertyGridTab.Events) return;

            if (Events != null)
            {
                _eventPicker.Events = Events;
            }
            else
            {
                // 宿主未注入 → 反射当前 SelectedObject 的公共事件；选空时清空，避免残留
                _eventPicker.Events = SelectedObject == null
                    ? null
                    : EventItemFactory.CreateEvents(SelectedObject);
            }
        }

        /// <summary>同步页签选中态与属性/事件页可见性（模板未含事件页部件时安全无操作）</summary>
        internal void UpdateTabVisualState()
        {
            try
            {
                if (_propertiesTabButton != null)
                    _propertiesTabButton.IsChecked = ActiveTab == PropertyGridTab.Properties;
                if (_eventsTabButton != null)
                    _eventsTabButton.IsChecked = ActiveTab == PropertyGridTab.Events;

                var showEvents = ShowEventsPage;
                var eventsActive = showEvents && ActiveTab == PropertyGridTab.Events;

                // 事件页可见性由宿主 Border 控制（内部 picker 跟随显示）
                if (_eventHost != null)
                    _eventHost.Visibility = eventsActive ? Visibility.Visible : Visibility.Collapsed;

                // 事件页激活时隐藏属性宿主（含集合/字典编辑器），切回时按当前模式恢复
                if (_normalHost != null)
                {
                    _normalHost.Visibility = eventsActive
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
                if (_collectionEditorHost != null && eventsActive)
                    _collectionEditorHost.Visibility = Visibility.Collapsed;

                if (_propertiesTabButton != null)
                    _propertiesTabButton.Visibility = showEvents ? Visibility.Visible : Visibility.Collapsed;
                if (_eventsTabButton != null)
                    _eventsTabButton.Visibility = showEvents ? Visibility.Visible : Visibility.Collapsed;

                // 切换条整体：ShowEventsPage=true 时显示（事件页激活也可见，可随时切回）
                if (_tabHost != null)
                    _tabHost.Visibility = showEvents ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                // 快速切换期间的竞态：忽略本轮，下一轮状态同步会自愈
            }
        }

        #endregion

        #region 核心逻辑

        /// <summary>刷新属性列表</summary>
        public void RefreshProperties()
        {
            if (_itemsControl == null && _collectionEditorHost == null) return;

            // 合并刷新：快速连选/高频切换时每次调用都排一个 Render 优先级回调（高于输入优先级），
            // 会排队多个全量反射重建、饿掉鼠标消息（宿主表现为"卡死"）。
            // 代数计数让旧回调直接作废，只有最新一轮真正重建。
            var generation = ++_refreshGeneration;
            IsLoading = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (generation != _refreshGeneration) return;   // 已有更新一轮的刷新在排队，本轮作废

                // 1.4.0：快速切换属性/事件页或连续换选中对象时，本回调可能在元素刚被折叠/卸载后执行；
                // 继续操作半卸载的视觉树（含加载动画时钟）会触发 clr 内部错误闪退。
                // 守卫：控件已脱离视觉树则直接收尾，不再刷新。
                if (!IsLoaded)
                {
                    IsLoading = false;
                    return;
                }

                try
                {
                    if (TryShowDictionaryEditor() || TryShowCollectionEditor())
                        return;

                    ShowNormalMode();

                    // 同类型换对象（切设备 / 切产品这类高频操作）走快速路径：行控件原地保留，
                    // 只把每个属性项的宿主换成新对象并刷新值，不重建整表。返回 true 表示已处理。
                    if (TryRebindSameShape(SelectedObject)) return;

                    _allProperties = PropertyItemFactory.CreateProperties(SelectedObject).Cast<IPropertyItem>().ToList();

                    // 内存释放：列表整体重建后，旧的选中引用（_selectedProperty 字段与 SelectedPropertyItem DP）
                    // 仍指向已不在新列表中的旧 PropertyItem 实例，会把旧对象及其 owner 引用链一直挂在本控件上
                    // （每次切换对象泄漏一份）。这里把失效选中清空，ApplyFilter 在 ShowDescription 模式下
                    // 会自动重选第一个属性，行为等价于 VS 属性窗口。
                    if (SelectedPropertyItem != null && !_allProperties.Contains(SelectedPropertyItem))
                        SelectedPropertyItem = null;
                    if (_selectedProperty != null && !_allProperties.Contains(_selectedProperty))
                        _selectedProperty = null;

                    foreach (var prop in _allProperties)
                    {
                        prop.ValueChanged -= OnPropertyValueValueChanged;
                        prop.ValueChanged += OnPropertyValueValueChanged;
                        prop.PropertyChanged -= OnPropertyItemPropertyChanged;
                        prop.PropertyChanged += OnPropertyItemPropertyChanged;
                        prop.ButtonClicked -= OnPropertyItemButtonClicked;
                        prop.ButtonClicked += OnPropertyItemButtonClicked;

                        // 网格级只读：把条目置为只读（子属性在懒加载时经 IsReadOnly 向下传播）
                        if (ReadOnly && prop is Controls.PropertyItem pi)
                            pi.IsReadOnly = true;
                    }

                    ApplyFilter();
                }
                catch
                {
                    // 元素快速显隐期间的竞态异常：吞掉防止升级为闪退（下次刷新会重建列表，无状态损失）
                }
                finally
                {
                    IsLoading = false;
                }
            }), DispatcherPriority.Render);
        }

        /// <summary>当 SelectedObject 直接是字典（IDictionary）时，内联显示字典编辑器</summary>
        private bool TryShowDictionaryEditor()
        {
            var obj = SelectedObject;
            if (obj == null || obj is string || !(obj is IDictionary dict))
                return false;

            var editor = new DictionaryEditorControl { Items = dict };

            if (_collectionEditorHost != null)
            {
                _collectionEditorHost.Content = editor;
                _collectionEditorHost.Visibility = Visibility.Visible;
            }
            if (_normalHost != null)
                _normalHost.Visibility = Visibility.Collapsed;

            return true;
        }

        private bool TryShowCollectionEditor()
        {
            var obj = SelectedObject;
            if (obj == null || !IsEditableCollection(obj))
                return false;

            var list = (IList)obj;
            var elementType = GetCollectionElementType(list);
            FrameworkElement editor;

            if (IsSimpleType(elementType))
            {
                editor = new SimpleCollectionEditorControl { Items = list };
            }
            else
            {
                editor = new CollectionEditorControl { Items = list };
            }

            if (_collectionEditorHost != null)
            {
                _collectionEditorHost.Content = editor;
                _collectionEditorHost.Visibility = Visibility.Visible;
            }
            if (_normalHost != null)
                _normalHost.Visibility = Visibility.Collapsed;

            return true;
        }

        private void ShowNormalMode()
        {
            if (_collectionEditorHost != null)
            {
                _collectionEditorHost.Content = null;
                _collectionEditorHost.Visibility = Visibility.Collapsed;
            }
            if (_normalHost != null)
            {
                // 1.4.0：事件页激活时保持属性宿主折叠（否则刷新属性时两层叠影）
                var eventsActive = ShowEventsPage && ActiveTab == PropertyGridTab.Events;
                _normalHost.Visibility = eventsActive ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private static bool IsEditableCollection(object obj)
        {
            if (obj is string || obj is IDictionary)
                return false;
            return obj is IList;
        }

        private static Type GetCollectionElementType(IList list)
        {
            var type = list.GetType();
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                if (args.Length > 0) return args[0];
            }
            if (list.Count > 0)
                return list[0]?.GetType() ?? typeof(object);
            return typeof(object);
        }

        private static bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return IsSimpleType(Nullable.GetUnderlyingType(type));

            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(TimeSpan)
                || type == typeof(Guid);
        }

        /// <summary>
        /// 换对象的快速刷新路径：新旧 <see cref="SelectedObject"/> 运行时类型相同、
        /// 搜索条件未变、且当前没有展开的子属性行时，属性元数据与可见行集合完全一致 ——
        /// 只需把每个属性项的宿主换成新对象并刷新值，**行控件、分类折叠状态、滚动位置全部原地保留**。
        ///
        /// 返回 false 表示条件不满足，调用方回落到原来的"反射重建整表"路径。
        /// 判据说明：
        /// · 类型相同 → PropertyInfo / 特性 / 枚举表 / 自定义编辑器全部一致，无需重新反射；
        /// · 无展开行 → 平铺行集合里没有从旧宿主的值上反射出来的子属性行（子属性必须重来，交给全量路径）；
        /// · 搜索条件相同 → 可见行集合不变，不需要重算过滤与分类分组。
        /// </summary>
        private bool TryRebindSameShape(object target)
        {
            var previous = _lastRefreshedObject;
            if (target == null || previous == null) return false;
            if (_allProperties == null || _allProperties.Count == 0) return false;
            if (_itemsControl == null || _flatRows == null) return false;

            if (ReferenceEquals(target, previous)) return true;   // 同一对象：值已在刷新时读过，无需重建
            if (target.GetType() != previous.GetType()) return false;

            if (!string.Equals(_lastFilterText ?? string.Empty, SearchText ?? string.Empty, StringComparison.Ordinal))
                return false;

            foreach (var item in _allProperties)
            {
                if (item is Controls.PropertyItem pi && pi.IsExpanded) return false;
            }

            foreach (var item in _allProperties)
            {
                (item as Controls.PropertyItem)?.RebindOwner(target);
            }

            _lastRefreshedObject = target;
            return true;
        }

        /// <summary>应用搜索过滤</summary>
        private void ApplyFilter()
        {
            if (_itemsControl == null) return;

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allProperties
                : _allProperties.Where(p =>
                    p.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    p.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    p.Category.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            var categories = PropertyItemFactory.GroupByCategory(filtered);
            GroupedCategories = categories;

            // 性能关键：把「类别 → 属性」两层嵌套压平成单层列表（类别头 + 其下的属性行），
            // 模板里再配 VirtualizingStackPanel 才能真的虚拟化。嵌套结构下内层 ItemsControl 会被
            // 赋予无限高度，VirtualizingStackPanel 不会回收容器、等于没开。
            // 实测依据：默认折叠类别（行不再生成）后单次重建从 ~253ms 降到 ~12ms，
            // 说明 95% 的耗时就是属性行的 UI 生成，只有虚拟化能同时保住"属性可见"和速度。
            foreach (var category in categories)
            {
                category.PropertyChanged -= OnCategoryPropertyChanged;
                category.PropertyChanged += OnCategoryPropertyChanged;
            }
            _flatRows = BuildFlatRows(categories, ShowCategoryHeaders);
            _flatRowCount = _flatRows.Count;

            // 行数自适应虚拟化：少行（<40）关掉让布局精确 ——
            // 虚拟化按平均行高估算 Extent，图片预览行（~200）混在文字行（~56）里
            // 估算偏大时，滚动条 Auto 照常出现且拖到底留一大片空白；
            // 几十行全量生成开销可忽略，大列表仍走虚拟化。
            if (_itemsControl != null)
            {
                var virtualize = EnableVirtualization && _flatRows.Count > 40;
                VirtualizingPanel.SetIsVirtualizing(_itemsControl, virtualize);
            }

            // 项滚动（CanContentScroll=True）虚拟化依赖"平均行高估算"决定滚动度量：
            // 面板首次布局用预测容器（无内容 ≈2px）估算，会把 95 行全部算进视口 → Scrollable=0 → 无滚动条。
            // 预热：先只放一行并强制布局，让 VirtualizingStackPanel 学到真实行高（56），滚动度量随即正确。
            // 代价：每次刷新多一次单行布局（约 1-2ms），换来虚拟化正常 + 滚动条准确。
            if (_flatRows.Count > 0)
            {
                _itemsControl.ItemsSource = new object[] { _flatRows[0] };
                _itemsControl.UpdateLayout();
            }
            _itemsControl.ItemsSource = _flatRows;

            if (ShowDescription && filtered.Count > 0 && SelectedPropertyItem == null)
            {
                SelectedPropertyItem = filtered[0];
            }

            // 记录本轮快照：下一次换对象时据此判断能否走原地换绑（见 TryRebindSameShape）
            _lastRefreshedObject = SelectedObject;
            _lastFilterText = SearchText ?? string.Empty;
        }

        /// <summary>定位列表 ScrollViewer（滚动诊断用）：模板内已命名 PART_ScrollViewer，直接取；</summary>
        private static ScrollViewer FindScrollViewer(ItemsControl itemsHost)
        {
            try
            {
                return (ScrollViewer)itemsHost.Template.FindName("PART_ScrollViewer", itemsHost);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>属性值变更时刷新</summary>
        private void OnPropertyValueValueChanged(object sender, EventArgs e)
        {
            foreach (var prop in _allProperties)
            {
                prop.Refresh();
            }

            // 1.4.0：对外广播属性值变更（设计器宿主可借此把修改写回自己的模型，
            // 如 WpfDesign 的 DesignItemProperty → XAML）
            if (sender is IPropertyItem changed)
            {
                PropertyValueChanged?.Invoke(this,
                    new PropertyValueChangedEventArgs(changed.Name, changed.Value));
            }
        }

        private void OnPropertyItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 扁平化列表下，属性项展开/折叠必须重建行集合（子属性行平铺插入/移除），
            // 不能再靠行模板里的嵌套 ItemsControl 绑定反映（嵌套巨行会让虚拟化估算乱跳）。
            if (e.PropertyName != nameof(PropertyItem.IsExpanded)) return;
            if (_suppressFlatRebuild || _itemsControl == null) return;

            if (GroupedCategories is IEnumerable<PropertyCategory> categories)
                _itemsControl.ItemsSource = BuildFlatRows(categories);
        }

        /// <summary>
        /// 处理属性项的按钮点击：先轻量刷新本网格的属性值，使 [Button] 的 Action 对模型的修改立即可见
        /// （集合/字典编辑器内嵌的网格同样会自刷新，解决“按钮在嵌套网格中看似无效”的问题）；
        /// 再触发实例事件 ButtonClicked 与全局事件 GlobalButtonClicked（后者可跨内嵌网格/弹窗被宿主接收）。
        /// </summary>
        private void OnPropertyItemButtonClicked(object sender, PropertyButtonClickEventArgs e)
        {
            if (_allProperties != null)
            {
                foreach (var prop in _allProperties)
                    prop.Refresh();
            }
            ButtonClicked?.Invoke(this, e);
            GlobalButtonClicked?.Invoke(this, e);
        }

        /// <summary>语言切换时刷新所有属性的本地化名称和描述</summary>
        private void OnLanguageChanged(object sender, EventArgs e)
        {
            foreach (var prop in _allProperties)
            {
                if (prop is PropertyItem item)
                    item.UpdateLocalization();
            }
            UpdateDescriptionBar();
            ApplyFilter();
        }

        /// <summary>选中属性（显示描述）</summary>
        internal void SelectProperty(PropertyItem item)
        {
            _selectedProperty = item;
            if (_descriptionTitle != null)
                _descriptionTitle.Text = item?.DisplayName ?? "";
            if (_descriptionText != null)
                _descriptionText.Text = item?.Description ?? "";
        }

        /// <summary>重置按钮点击</summary>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is PropertyItem item)
            {
                item.ResetToDefault();
                SelectProperty(item);
            }
        }

        /// <summary>重置选中属性为默认值</summary>
        public void ResetSelectedToDefault()
        {
            _selectedProperty?.ResetToDefault();
        }

        /// <summary>重置所有属性为默认值</summary>
        public void ResetAllToDefault()
        {
            foreach (var prop in _allProperties)
                prop.ResetToDefault();
        }

        /// <summary>展开所有分类和可展开属性</summary>
        public void ExpandAll()
        {
            SetAllExpanded(true);
        }

        /// <summary>折叠所有分类和可展开属性</summary>
        public void CollapseAll()
        {
            SetAllExpanded(false);
        }

        /// <summary>把「类别 → 属性」压平成单层列表：类别头一项，后面跟着该类别展开时的属性行；
        /// 可展开属性展开时，其子属性（递归）作为独立行平铺插入（带缩进层级）。
        /// 扁平化是虚拟化的前提：所有行都在同一个 ItemsControl 里、等高，
        /// VirtualizingStackPanel 的估算才稳定，滚动条不会忽长忽短；
        /// 子属性不再嵌套在父行内部的 ItemsControl 里（嵌套的父行会变成巨行，
        /// Recycling 复用它给普通行时高度残留，导致滚动范围乱跳）。
        /// internal：PropertyGridPro 复用同一套压平逻辑，避免两份实现各自漂移。</summary>
        internal static List<object> BuildFlatRows(IEnumerable<PropertyCategory> categories, bool includeCategoryHeaders = true, bool markGroupFirst = false)
        {
            var rows = new List<object>();
            if (categories == null) return rows;

            var firstGroup = true;
            foreach (var category in categories)
            {
                // 不显示分类头时（ShowCategoryHeaders=false）：分组对象仍参与过滤与排序，
                // 只是不再往行集合里插"类别头"这一项 —— 视觉上就是全部属性平铺往下排。
                if (includeCategoryHeaders)
                {
                    // 分类之间的"缝"：非首个分类标记画分隔线（首个上方是页头/标题留白）
                    category.ShowGroupSeparator = !firstGroup;
                    rows.Add(category);
                    if (!category.IsExpanded) continue;
                }
                var isFirstOfGroup = true;
                var lastItem = category.Items.Count > 0 ? category.Items[category.Items.Count - 1] as PropertyItem : null;
                foreach (var item in category.Items)
                {
                    if (item is PropertyItem pi)
                    {
                        // 组首行标记两种布局都需要：
                        //  - 普通/紧凑列表：行模板据此给卡片补上边框 + 上圆角
                        //    （无分类头平铺时首行直接顶在标题下，不补就是直角）；
                        //  - 表单式：分类名由首行左列渲染（markGroupFirst）。
                        // 之前只在 markGroupFirst 时赋值，普通版恒 false → 上圆角永不生效。
                        pi.IsGroupFirst = isFirstOfGroup;
                        // 分类"卡片"的收口：每组最后一行画下圆角
                        pi.IsGroupLast = ReferenceEquals(pi, lastItem);
                        // 分类分隔线：每个分组首行都画（含第一个分组）——
                        // 线的实际位置在「分类名下方、该组第一行卡片上方」，
                        // 第一个分类也该有，否则视觉上第一个分类头像是"没打完"（demo 反馈）。
                        // 不再限定 markGroupFirst：不显示分类头时同样需要这条蓝线分隔各组。
                        pi.ShowGroupSeparator = isFirstOfGroup;
                    }
                    isFirstOfGroup = false;
                    AppendItemRecursive(rows, item, 0);
                }
                firstGroup = false;
            }
            return rows;
        }

        private static void AppendItemRecursive(List<object> rows, IPropertyItem item, int level)
        {
            if (item is PropertyItem pi) pi.Level = level;
            rows.Add(item);
            if (item.IsExpandable && item.IsExpanded)
            {
                var children = item.ChildProperties; // 惰性加载：首次展开才反射子属性
                if (children != null)
                {
                    foreach (var child in children)
                        AppendItemRecursive(rows, child, level + 1);
                }
            }
        }

        /// <summary>批量展开/折叠期间置 true：否则每个类别各自触发一次列表重建</summary>
        private bool _suppressFlatRebuild;

        private void OnCategoryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PropertyCategory.IsExpanded)) return;
            if (_suppressFlatRebuild || _itemsControl == null) return;

            if (GroupedCategories is IEnumerable<PropertyCategory> categories)
                _itemsControl.ItemsSource = BuildFlatRows(categories);
        }

        /// <summary>批量展开/折叠所有分类（类别头）。嵌套层不参与"全部展开"——
        /// 递归展开会触发全部子属性反射构建（实测 498ms 卡顿），用户明确要求嵌套层保持折叠；
        /// 需要展开某个嵌套项时单点其箭头即可（单点只构建该链路的子属性）。</summary>
        private void SetAllExpanded(bool expanded)
        {
            if (_allProperties == null) return;

            _suppressFlatRebuild = true;
            try
            {
                if (GroupedCategories != null)
                {
                    foreach (var category in GroupedCategories.OfType<PropertyCategory>())
                    {
                        category.IsExpanded = expanded;
                    }
                }
            }
            finally
            {
                _suppressFlatRebuild = false;
            }

            // 批量改完统一重建一次（每个类别单独触发会重复重建整个列表）
            if (_itemsControl != null && GroupedCategories is IEnumerable<PropertyCategory> cats)
                _itemsControl.ItemsSource = BuildFlatRows(cats);
        }

        /// <summary>递归设置属性项及其子属性的展开状态（扁平化后子属性行也参与展开）</summary>
        private static void SetExpandedRecursive(IPropertyItem item, bool expanded)
        {
            if (!item.IsExpandable) return;

            item.IsExpanded = expanded;

            // 折叠：收起即可，不触碰子属性。
            // 关键：ChildProperties getter 会在 _childProperties==null 时无条件触发 LoadChildProperties
            // （反射构造全部子属性）。折叠若也访问它，等于把所有子属性白白构建一遍 —— 是"折叠也卡死"的原因。
            if (!expanded) return;

            var children = item.ChildProperties;
            if (children != null)
            {
                foreach (var child in children)
                    SetExpandedRecursive(child, expanded);
            }
        }

        #endregion
    }

    /// <summary>属性值变更事件参数（1.4.0 新增）</summary>
    public class PropertyValueChangedEventArgs : EventArgs
    {
        /// <summary>被修改的属性名</summary>
        public string PropertyName { get; }

        /// <summary>修改后的值（可能为 null）</summary>
        public object NewValue { get; }

        public PropertyValueChangedEventArgs(string propertyName, object newValue)
        {
            PropertyName = propertyName;
            NewValue = newValue;
        }
    }
    /// <summary>PropertyGrid 当前激活页（1.4.0 新增）</summary>
    public enum PropertyGridTab
    {
        /// <summary>属性页（默认）</summary>
        Properties,
        /// <summary>事件页（仅 ShowEventsPage=true 时可用）</summary>
        Events
    }
}