using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PropertyGridLib.Controls;
using PropertyGridLib.Localization;

namespace PropertyGridLib
{
    /// <summary>
    /// PropertyGridPro：面向「参数配置面板」场景的平铺属性面板。
    ///
    /// 与 <see cref="PropertyGrid"/> 的差异：
    /// 1) 纵向平铺列表，每行 = 属性名 + 说明描述 + 值编辑器，<b>不折叠展开</b>（无 Expander、无展开箭头）；
    /// 2) 内置属性搜索框，支持按「显示名 / 属性名 / 分类 / 描述」多关键词（空格分隔，需全部命中）过滤；
    /// 3) 自带深色「参数配置」外观（面板配色由 Pro* 系列画刷控制，宿主可覆盖这些 Key 换肤）；
    /// 4) 无底部描述栏、无集合/字典整屏模式 —— 描述文字直接跟随每一行展示。
    ///
    /// 与 <see cref="PropertyGrid"/> 共用：属性项模型（<see cref="PropertyItemFactory"/> / <see cref="PropertyItem"/>）、
    /// 编辑器模板选择器（EditorSelector）、全部内置编辑器模板（Slider / 下拉 / 复选框 / 颜色 / 集合 / 字典 /
    /// 公式 / 多选 / 按钮 / 文件 / 目录 ...）、重置命令、本地化机制、[Button] 全局事件（GlobalButtonClicked）。
    /// </summary>
    public class PropertyGridPro : Control
    {
        #region 字段

        private readonly List<IPropertyItem> _allProperties = new List<IPropertyItem>();
        private string _matchCountFormat = string.Empty;

        /// <summary>属性平铺列表（虚拟化宿主，模板内 PART_ItemsControl）</summary>
        private ItemsControl? _itemsControl;

        /// <summary>列表滚动宿主（模板内 PART_ScrollViewer）：展开重建行集合时用它恢复滚动位置</summary>
        private ScrollViewer? _scrollViewer;

        #endregion

        #region 构造 / 生命周期

        static PropertyGridPro()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(typeof(PropertyGridPro)));
        }

        public PropertyGridPro()
        {
            // 点击任意属性行 → 记录当前选中项（供 ResetSelectedToDefault / 宿主读取）
            PreviewMouseLeftButtonDown += OnRowClicked;
            LocalizationManager.LanguageChanged += OnLanguageChanged;
            RefreshLocalizedTexts();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
            // 模板重应用时内层 ItemsControl 可能是新实例：强制丢弃旧引用，
            // 让 RebuildRows 惰性重新获取（此时内层模板多半尚未实例化，FindName 拿不到）。
            _scrollViewer = null;
            ApplyVirtualizationMode();

            // 不能在这里同步 RefreshProperties：此刻窗口首轮布局未定型，刷新里的
            // "首行预热"会强制 UpdateLayout，虚拟化面板用中间态视口算出错误偏移并记住，
            // 实测新开窗口直接落在列表中部、点「从模型刷新」跳到底部。
            // 推迟到布局/Loaded 之后再刷新（与基类 RefreshProperties 的 BeginInvoke 同思路），
            // 滚动位置另由 RebuildRows 里显式恢复兜底。
            Dispatcher.BeginInvoke(new Action(RefreshProperties), DispatcherPriority.Loaded);
        }

        private void OnRowClicked(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            while (source != null && source != this)
            {
                if (source is FrameworkElement fe && fe.DataContext is IPropertyItem item)
                {
                    SelectedPropertyItem = item;
                    return;
                }
                source = VisualTreeHelper.GetParent(source);
            }
        }

        #endregion

        #region 依赖属性

        /// <summary>要展示的对象</summary>
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(null, OnSelectedObjectChanged));

        /// <summary>搜索关键字（多关键词以空格分隔，需全部命中）</summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

        /// <summary>过滤后的分组结果（供模板绑定）</summary>
        public static readonly DependencyProperty GroupedCategoriesProperty =
            DependencyProperty.Register(nameof(GroupedCategories), typeof(IEnumerable), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(null));

        /// <summary>属性列表是否启用 UI 虚拟化（默认 true，与基类 PropertyGrid 一致）。
        /// 宿主自定义 RowTemplate 出现滚动串值时可设为 false 回退到全量生成。</summary>
        public static readonly DependencyProperty EnableVirtualizationProperty =
            DependencyProperty.Register(nameof(EnableVirtualization), typeof(bool), typeof(PropertyGridPro),
                new PropertyMetadata(true, OnEnableVirtualizationChanged));

        /// <summary>当前选中的属性项</summary>
        public static readonly DependencyProperty SelectedPropertyItemProperty =
            DependencyProperty.Register(nameof(SelectedPropertyItem), typeof(IPropertyItem), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(null));

        /// <summary>面板标题</summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>面板说明（标题下方灰色描述）</summary>
        public static readonly DependencyProperty HeaderDescriptionProperty =
            DependencyProperty.Register(nameof(HeaderDescription), typeof(string), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>来源信息（更小字号的一行灰色文字）</summary>
        public static readonly DependencyProperty SourceTextProperty =
            DependencyProperty.Register(nameof(SourceText), typeof(string), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>是否显示标题区（标题 + 说明 + 来源）</summary>
        public static readonly DependencyProperty ShowHeaderProperty =
            DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(true));

        /// <summary>是否显示搜索框</summary>
        public static readonly DependencyProperty ShowSearchBarProperty =
            DependencyProperty.Register(nameof(ShowSearchBar), typeof(bool), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(true));

        /// <summary>是否显示分类标题（关闭后所有属性平铺为单一列表）</summary>
        public static readonly DependencyProperty ShowCategoryHeadersProperty =
            DependencyProperty.Register(nameof(ShowCategoryHeaders), typeof(bool), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(true));

        /// <summary>值编辑器区域宽度（像素）</summary>
        public static readonly DependencyProperty EditorWidthProperty =
            DependencyProperty.Register(nameof(EditorWidth), typeof(double), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(260d));

        /// <summary>属性行模板；子属性递归渲染时复用同一模板，实现无限嵌套且样式统一</summary>
        public static readonly DependencyProperty RowTemplateProperty =
            DependencyProperty.Register(nameof(RowTemplate), typeof(DataTemplate), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(null));

        /// <summary>当前命中的属性条数</summary>
        public static readonly DependencyProperty MatchedCountProperty =
            DependencyProperty.Register(nameof(MatchedCount), typeof(int), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(0));

        /// <summary>命中条数的提示文本（如 "共 5 项"）</summary>
        public static readonly DependencyProperty MatchCountTextProperty =
            DependencyProperty.Register(nameof(MatchCountText), typeof(string), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>是否无匹配结果（显示空状态提示）</summary>
        public static readonly DependencyProperty HasNoResultProperty =
            DependencyProperty.Register(nameof(HasNoResult), typeof(bool), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// 是否显示属性类型提示（如 "(int)"）。
        /// 与基类 PropertyGrid.ShowTypeHint 语义一致：公式绑定区 / 公式树节点的类型提示受此开关控制。
        /// </summary>
        public static readonly DependencyProperty ShowTypeHintProperty =
            DependencyProperty.Register(nameof(ShowTypeHint), typeof(bool), typeof(PropertyGridPro),
                new FrameworkPropertyMetadata(true));

        /// <summary>要展示的对象；赋值后自动反射生成属性行</summary>
        public object? SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        /// <summary>搜索关键字</summary>
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        /// <summary>过滤后的分组结果</summary>
        public IEnumerable? GroupedCategories
        {
            get => (IEnumerable?)GetValue(GroupedCategoriesProperty);
            private set => SetValue(GroupedCategoriesProperty, value);
        }

        /// <summary>
        /// 属性列表是否启用 UI 虚拟化（默认 true）。与基类 PropertyGrid 同款实现：
        /// 只生成视口内的属性行并复用容器；设为 false 回到「全量生成」行为，
        /// 用于宿主自定义 RowTemplate 出现滚动串值时的回退。
        /// </summary>
        public bool EnableVirtualization
        {
            get => (bool)GetValue(EnableVirtualizationProperty);
            set => SetValue(EnableVirtualizationProperty, value);
        }

        private static void OnEnableVirtualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pg = (PropertyGridPro)d;
            // 运行期只切 IsVirtualizing（WPF 允许 Measure 后修改）：
            // false → VirtualizingStackPanel 退化为全量生成，等效旧行为。
            // 不能动 VirtualizationMode —— Measure 后再改会抛 InvalidOperationException，
            // 回收模式由模板 XAML 里的 VirtualizationMode="Recycling" 在模板应用时一次性设定。
            if (pg._itemsControl != null)
                VirtualizingPanel.SetIsVirtualizing(pg._itemsControl, (bool)e.NewValue);
        }

        /// <summary>把 EnableVirtualization 落到实际列表控件上（OnApplyTemplate 时调用一次）。
        /// 只切 IsVirtualizing，VirtualizationMode 运行期不可改（见 OnEnableVirtualizationChanged）。</summary>
        private void ApplyVirtualizationMode()
        {
            if (_itemsControl == null) return;

            VirtualizingPanel.SetIsVirtualizing(_itemsControl, EnableVirtualization);
        }

        /// <summary>当前选中的属性项</summary>
        public IPropertyItem? SelectedPropertyItem
        {
            get => (IPropertyItem?)GetValue(SelectedPropertyItemProperty);
            set => SetValue(SelectedPropertyItemProperty, value);
        }

        /// <summary>面板标题</summary>
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>面板说明</summary>
        public string HeaderDescription
        {
            get => (string)GetValue(HeaderDescriptionProperty);
            set => SetValue(HeaderDescriptionProperty, value);
        }

        /// <summary>来源信息</summary>
        public string SourceText
        {
            get => (string)GetValue(SourceTextProperty);
            set => SetValue(SourceTextProperty, value);
        }

        /// <summary>是否显示标题区</summary>
        public bool ShowHeader
        {
            get => (bool)GetValue(ShowHeaderProperty);
            set => SetValue(ShowHeaderProperty, value);
        }

        /// <summary>是否显示搜索框</summary>
        public bool ShowSearchBar
        {
            get => (bool)GetValue(ShowSearchBarProperty);
            set => SetValue(ShowSearchBarProperty, value);
        }

        /// <summary>是否显示分类标题</summary>
        public bool ShowCategoryHeaders
        {
            get => (bool)GetValue(ShowCategoryHeadersProperty);
            set => SetValue(ShowCategoryHeadersProperty, value);
        }

        /// <summary>值编辑器区域宽度</summary>
        public double EditorWidth
        {
            get => (double)GetValue(EditorWidthProperty);
            set => SetValue(EditorWidthProperty, value);
        }

        /// <summary>属性行模板（子属性递归复用以实现无限嵌套）</summary>
        public DataTemplate? RowTemplate
        {
            get => GetValue(RowTemplateProperty) as DataTemplate;
            set => SetValue(RowTemplateProperty, value);
        }

        /// <summary>当前命中的属性条数</summary>
        public int MatchedCount
        {
            get => (int)GetValue(MatchedCountProperty);
            private set => SetValue(MatchedCountProperty, value);
        }

        /// <summary>命中条数提示文本</summary>
        public string MatchCountText
        {
            get => (string)GetValue(MatchCountTextProperty);
            private set => SetValue(MatchCountTextProperty, value);
        }

        /// <summary>是否无匹配结果</summary>
        public bool HasNoResult
        {
            get => (bool)GetValue(HasNoResultProperty);
            private set => SetValue(HasNoResultProperty, value);
        }

        /// <summary>
        /// 是否显示属性类型提示（如 "(int)"），默认 true。
        /// 与基类 PropertyGrid.ShowTypeHint 行为一致，作用于公式绑定区与公式树节点的类型提示。
        /// </summary>
        public bool ShowTypeHint
        {
            get => (bool)GetValue(ShowTypeHintProperty);
            set => SetValue(ShowTypeHintProperty, value);
        }

        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyGridPro)d).RefreshProperties();
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyGridPro)d).ApplyFilter();
        }

        #endregion

        #region 事件

        /// <summary>属性行上的 [Button] 被点击（本实例内）</summary>
        public event EventHandler<PropertyButtonClickEventArgs>? ButtonClicked;

        /// <summary>任意属性值发生变化（宿主刷新"实时取值"等场景使用）</summary>
        public event EventHandler? PropertyValueChanged;

        #endregion

        #region 核心逻辑

        /// <summary>刷新属性列表（重新反射 SelectedObject）</summary>
        public void RefreshProperties()
        {
            DetachProperties();

            var source = SelectedObject;
            var items = source == null
                ? new List<PropertyItem>()
                : PropertyItemFactory.CreateProperties(source);
            foreach (var prop in items)
            {
                prop.ValueChanged += OnPropertyItemValueChanged;
                prop.PropertyChanged += OnPropertyItemPropertyChanged;
                prop.ButtonClicked += OnPropertyItemButtonClicked;
                _allProperties.Add(prop);
            }

            ApplyFilter();
        }

        /// <summary>应用搜索过滤并重建分组</summary>
        private void ApplyFilter()
        {
            var keyword = (SearchText ?? string.Empty).Trim();
            var filtered = string.IsNullOrEmpty(keyword)
                ? _allProperties
                : _allProperties.Where(p => Matches(p, keyword)).ToList();

            var categories = PropertyItemFactory.GroupByCategory(filtered);
            GroupedCategories = categories;

            // 与基类同款性能处理：把「类别 → 属性」两层嵌套压平成单层列表
            // （类别头一项 + 其下的属性行），模板里配 VirtualizingStackPanel 才能真正虚拟化。
            // 嵌套结构下内层 ItemsControl 会被赋予无限高度，虚拟化不会生效。
            RebuildRows(categories);

            MatchedCount = filtered.Count;
            HasNoResult = filtered.Count == 0;
            UpdateMatchCountText();

            // 选中项若被过滤掉则清空，否则默认选中第一条
            if (SelectedPropertyItem != null && !filtered.Contains(SelectedPropertyItem))
                SelectedPropertyItem = null;
            if (SelectedPropertyItem == null && filtered.Count > 0)
                SelectedPropertyItem = filtered[0];
        }

        /// <summary>多关键词匹配：空格分隔的每个关键词，须在 显示名/属性名/分类/描述 任一字段中命中</summary>
        private static bool Matches(IPropertyItem item, string keyword)
        {
            var parts = keyword.Split(new[] { ' ', '\t', ',', '，', ';', '；' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return true;

            foreach (var part in parts)
            {
                if (!Contains(item.DisplayName, part) &&
                    !Contains(item.Name, part) &&
                    !Contains(item.Category, part) &&
                    !Contains(item.Description, part))
                    return false;
            }
            return true;
        }

        private static bool Contains(string? source, string keyword)
            => source != null && source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;

        private void OnPropertyItemValueChanged(object? sender, EventArgs e)
        {
            // 值变更后轻量刷新所有属性行（联动只读项、按钮文本等），并通知宿主
            foreach (var prop in _allProperties)
                prop.Refresh();

            // 转发具体属性项（sender）给宿主 —— 与基类 PropertyValueChanged 语义对齐，
            // 宿主可据此拿到属性名与新值写回设计面
            PropertyValueChanged?.Invoke(sender is PropertyItem pi ? pi : this, EventArgs.Empty);
        }

    private void OnPropertyItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 扁平化列表下，属性项展开/折叠必须重建行集合（子属性行平铺插入/移除）。
        // 与基类同款轻量路径：只重建 ItemsSource —— 展开不改变过滤结果，
        // 不能走 ApplyFilter（那会重新 GroupByCategory、重算命中数、重置选中项，
        // 每次展开瞬间整列表卡顿）。preserveScroll:true 保持滚动位置不跳。
        if (e.PropertyName != nameof(PropertyItem.IsExpanded)) return;
        if (_suppressFlatRebuild || _itemsControl == null) return;

        if (GroupedCategories is IEnumerable<PropertyCategory> categories)
            RebuildRows(categories, preserveScroll: true);
    }

    /// <summary>把「类别 → 属性」压平后赋给列表。</summary>
    /// <param name="categories">分组结果</param>
    /// <param name="preserveScroll">保留参数（两种路径现在都会恢复滚动位置）。
    /// false = 全量刷新（搜索/换对象/语言切换）：走与基类同款"首行预热"——
    /// CanContentScroll 项滚动下 VirtualizingStackPanel 首次布局用预测容器（≈2px 高）估算，
    /// 会把全部行算进视口 → Scrollable=0 无滚动条；先放一行强制布局让面板学到真实行高，
    /// 再放全量列表。true = 展开/批量展开：只换列表，不预热。</param>
    /// <remarks>两条路径都在换列表后显式恢复滚动偏移：预热的中途布局、以及
    /// RefreshProperties 在 OnApplyTemplate 里同步执行（窗口首轮布局未定）时，
    /// 虚拟化面板自己记的偏移不可靠（实测新开窗口落在列表中部、刷新跳到底部），
    /// 偏移必须由这里显式拨回，不能指望面板自己保持。</remarks>
    private void RebuildRows(IEnumerable<PropertyCategory> categories, bool preserveScroll = false)
    {
        if (_itemsControl == null) return;

        // 惰性获取：OnApplyTemplate 时内层 ItemsControl 自己的模板还没实例化，
        // FindName 拿不到 PART_ScrollViewer（拿到 null 就没人恢复偏移了），布局后这里才拿得到。
        if (_scrollViewer == null)
            _scrollViewer = _itemsControl.Template?.FindName("PART_ScrollViewer", _itemsControl) as ScrollViewer;

        var rows = PropertyGrid.BuildFlatRows(categories);
        double offset = _scrollViewer?.VerticalOffset ?? 0;

        // 预热只用于"首次填充"（offset=0）：修 CanContentScroll 下 VSP 首次布局用预测容器
        // （≈2px）估算导致的无滚动条问题。已滚动的重建绝不预热 —— 1 行中间态会把面板内部
        // 偏移搅乱，VSP 随后自己恢复到错误位置（实测 captured=12 restored=73=滚动最大值）。
        if (!preserveScroll && offset <= 0 && rows.Count > 0)
        {
            _itemsControl.ItemsSource = new object[] { rows[0] };
            _itemsControl.UpdateLayout();
        }
        _itemsControl.ItemsSource = rows;

        if (offset > 0)
        {
            // 恢复滚动位置必须"补刀"：同步拨一次，再在 Loaded 优先级（所有布局/渲染收尾后）再拨一次。
            // VSP 会在后续布局里重新套用自己记住的偏移，同步那次会被它覆盖，只有最后一次能留下来。
            _scrollViewer?.UpdateLayout();
            _scrollViewer?.ScrollToVerticalOffset(offset);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _scrollViewer?.ScrollToVerticalOffset(offset);
            }), DispatcherPriority.Loaded);
        }
    }

        private void OnPropertyItemButtonClicked(object? sender, PropertyButtonClickEventArgs e)
        {
            foreach (var prop in _allProperties)
                prop.Refresh();

            ButtonClicked?.Invoke(this, e);
            // 转发全局事件：宿主即使内嵌在弹窗/其他容器中也能统一接收
            PropertyGrid.RaiseGlobalButtonClicked(this, e);
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            foreach (var prop in _allProperties)
            {
                if (prop is PropertyItem item)
                    item.UpdateLocalization();
            }

            RefreshLocalizedTexts();
            ApplyFilter();
        }

        private void DetachProperties()
        {
            foreach (var prop in _allProperties)
            {
                prop.ValueChanged -= OnPropertyItemValueChanged;
                prop.PropertyChanged -= OnPropertyItemPropertyChanged;
                prop.ButtonClicked -= OnPropertyItemButtonClicked;
            }
            _allProperties.Clear();
            SelectedPropertyItem = null;
        }

        private void RefreshLocalizedTexts()
        {
            _matchCountFormat = LocalizationManager.GetString("ProMatchedCount");
            UpdateMatchCountText();
        }

        private void UpdateMatchCountText()
        {
            if (string.IsNullOrEmpty(_matchCountFormat))
                _matchCountFormat = LocalizationManager.GetString("ProMatchedCount");

            if (MatchedCount <= 0 || string.IsNullOrEmpty(SearchText))
            {
                MatchCountText = string.Empty;
                return;
            }

            try
            {
                MatchCountText = string.Format(_matchCountFormat, MatchedCount);
            }
            catch (FormatException)
            {
                MatchCountText = MatchedCount.ToString();
            }
        }

        /// <summary>重置选中属性为默认值</summary>
        public void ResetSelectedToDefault()
        {
            SelectedPropertyItem?.ResetToDefault();
        }

        /// <summary>重置全部属性为默认值</summary>
        public void ResetAllToDefault()
        {
            foreach (var prop in _allProperties)
                prop.ResetToDefault();
        }

        /// <summary>展开/折叠全部可展开属性（嵌套子属性仍支持折叠，顶层行始终平铺）</summary>
        public void SetSubPropertiesExpanded(bool expanded)
        {
            _suppressFlatRebuild = true;
            try
            {
                foreach (var prop in _allProperties)
                    SetExpandedRecursive(prop, expanded);
            }
            finally
            {
                _suppressFlatRebuild = false;
            }

            // 批量改完统一重建一次（每个属性单独触发会重复重建整个列表）；
            // 与基类 SetAllExpanded 同款只重建 ItemsSource —— 批量展开不改变过滤结果，
            // 走 ApplyFilter 会多做一轮分组/命中数/选中项重算；保持滚动位置不跳。
            if (_itemsControl != null && GroupedCategories is IEnumerable<PropertyCategory> cats)
                RebuildRows(cats, preserveScroll: true);
        }

        /// <summary>批量展开/折叠期间置 true：否则每个属性项各自触发一次列表重建</summary>
        private bool _suppressFlatRebuild;

        private static void SetExpandedRecursive(IPropertyItem item, bool expanded)
        {
            if (!item.IsExpandable) return;

            item.IsExpanded = expanded;

            // 折叠：收起即可，不触碰 ChildProperties —— getter 会在未加载时反射构建全部子属性，
            // 折叠也访问它等于把所有子属性白白构建一遍（与基类 SetExpandedRecursive 同款处理）。
            if (!expanded) return;

            // ChildProperties 可能为 null：惰性加载条件是 IsExpandable && Value != null，
            // 可展开但当前值为 null 的属性返回 null，直接 foreach 会 NRE 崩溃。
            var children = item.ChildProperties;
            if (children == null) return;

            foreach (var child in children)
                SetExpandedRecursive(child, expanded);
        }

        #endregion
    }
}
