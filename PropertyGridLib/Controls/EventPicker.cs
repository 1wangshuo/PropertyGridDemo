using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 事件筛选选择器：独立的事件浏览/筛选/绑定控件（PropertyGridLib 1.4.0 新增）。
    /// - 事件列表（搜索过滤 + 分类分组）；
    /// - 双击事件行或点击 ⚡ 按钮触发 <see cref="EditRequested"/>（宿主打开代码编辑器，如 CodeForge）；
    /// - <see cref="SelectedObject"/> 自动反射公共事件，或由宿主直接注入 <see cref="Events"/>；
    /// - 常与 <see cref="PropertyGrid"/> 的双页模式（ShowEventsPage=true）配合使用。
    /// </summary>
    public class EventPicker : Control
    {
        static EventPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EventPicker),
                new FrameworkPropertyMetadata(typeof(EventPicker)));

            // 行内 EventEditorControl 的 ⚡ 请求冒泡到此：统一走 EditRequested
            EventManager.RegisterClassHandler(typeof(EventPicker),
                EventEditorControl.EditRequestedEvent, new RoutedEventHandler(OnRowEditRequested));
        }

        public EventPicker()
        {
            FilteredEvents = new ObservableCollection<EventItem>();
            EventCategories = new ObservableCollection<EventCategory>();

            // 1.4.0：搜索防抖 —— 高频输入（连续打字/清空）时合并重建，避免每键一次全量刷新造成卡顿
            _searchDebounce = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _searchDebounce.Tick += (s, e) => { _searchDebounce.Stop(); ApplyFilter(); };
        }

        private readonly System.Windows.Threading.DispatcherTimer _searchDebounce;

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 防抖：不停表先重启（合并 200ms 内的连续变更）
            var picker = (EventPicker)d;
            picker._searchDebounce.Stop();
            picker._searchDebounce.Start();
        }

        private static void OnRowEditRequested(object sender, RoutedEventArgs e)
        {
            var picker = (EventPicker)sender;
            if (e is EventEditRoutedEventArgs ea && picker.EditRequested != null)
            {
                picker.EditRequested(picker, new EventItemEventArgs(ea.Item));
                e.Handled = true;
            }
        }

        // ---------------- 模板部件 ----------------

        private ItemsControl _itemsControl;
        private ScrollViewer _scrollViewer;

        /// <summary>模板部件未就绪 / 数据未变化时无需重建</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
            // 模板重应用时强制重取（内层模板此时尚未实例化，RebuildRows 里惰性获取）
            _scrollViewer = null;

            // 不能在这里同步重建：此刻布局未定型，预热会扰动虚拟化面板偏移（与 PropertyGrid 同款问题）。
            // 推迟到 Loaded 优先级，滚动位置由 RebuildRows 显式恢复兜底。
            Dispatcher.BeginInvoke(new Action(ApplyFilter), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ---------------- 数据注入 ----------------

        /// <summary>事件数据源（宿主自定义 EventItem 列表；设置后忽略 SelectedObject 反射）</summary>
        public IList<EventItem> Events
        {
            get => (IList<EventItem>)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        public static readonly DependencyProperty EventsProperty =
            DependencyProperty.Register(nameof(Events), typeof(IList<EventItem>), typeof(EventPicker),
                new PropertyMetadata(null, OnEventsChanged));

        private static void OnEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (EventPicker)d;
            // 数据源变更后旧的选中事件已不在列表里，清掉底部描述栏
            picker.SelectedEvent = null;
            // 数据源变更立即刷新（无防抖：只在换选中控件时发生一次）
            picker.ApplyFilter();
        }

        /// <summary>当前选中的事件行（点击事件行时更新）；底部描述栏显示它的名称与说明。
        /// 与属性页 SelectedPropertyItem → 描述栏 同款语义。</summary>
        public EventItem SelectedEvent
        {
            get => (EventItem)GetValue(SelectedEventProperty);
            set => SetValue(SelectedEventProperty, value);
        }

        public static readonly DependencyProperty SelectedEventProperty =
            DependencyProperty.Register(nameof(SelectedEvent), typeof(EventItem), typeof(EventPicker),
                new PropertyMetadata(null));

        /// <summary>目标对象：设置后自动反射其公共事件填充 <see cref="Events"/></summary>
        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(EventPicker),
                new PropertyMetadata(null, OnSelectedObjectChanged));

        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (EventPicker)d;
            picker.Events = e.NewValue == null
                ? null
                : EventItemFactory.CreateEvents(e.NewValue);
        }

        // ---------------- 过滤 / 分组 ----------------

        /// <summary>搜索文本（按事件显示名/名称过滤）</summary>
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(EventPicker),
                new PropertyMetadata(string.Empty, OnSearchTextChanged));

        /// <summary>过滤后的事件（平铺，按分类+名称排序；供宿主编程访问）</summary>
        public ObservableCollection<EventItem> FilteredEvents { get; }

        /// <summary>过滤后的事件分类分组（供宿主编程访问；UI 走 FlatRows 平铺行）</summary>
        public ObservableCollection<EventCategory> EventCategories { get; }

        private void ApplyFilter()
        {
            FilteredEvents.Clear();
            EventCategories.Clear();
            var all = Events;
            if (all == null)
            {
                _itemsControl?.SetCurrentValue(ItemsControl.ItemsSourceProperty, null);
                return;
            }

            IEnumerable<EventItem> seq = all;
            if (!string.IsNullOrWhiteSpace(SearchText))
                seq = seq.Where(e =>
                    e.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    e.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);

            var list = seq.ToList();
            foreach (var e in list)
                FilteredEvents.Add(e);

            foreach (var g in list.GroupBy(e => e.Category)
                .OrderBy(g => PropertyItemFactory.IsMiscCategory(g.Key) ? 1 : 0)  // 杂项/其他 分类排最后
                .ThenBy(g => g.Key, NaturalStringComparer.Instance))
            {
                var category = new EventCategory(g.Key,
                    g.OrderBy(e => e.DisplayName, NaturalStringComparer.Instance).ToList());

                // 搜索/换数据源都会重建分类对象，这里按名字回灌上次的折叠状态，
                // 否则用户每敲一个搜索字符，折叠过的分类都会被重新弹开。
                if (_collapsedCategories.Contains(category.Name))
                    category.IsExpanded = false;

                category.PropertyChanged += OnEventCategoryPropertyChanged;
                EventCategories.Add(category);
            }

            RebuildRowsFromCategories();
        }

        /// <summary>用户折叠过的分类名（分类对象每次重建都会换新，折叠状态按名字跨重建保留）</summary>
        private readonly HashSet<string> _collapsedCategories = new HashSet<string>(StringComparer.Ordinal);

        private void OnEventCategoryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(EventCategory.IsExpanded)) return;
            if (!(sender is EventCategory category)) return;

            if (category.IsExpanded) _collapsedCategories.Remove(category.Name);
            else _collapsedCategories.Add(category.Name);

            // 与属性页 OnCategoryPropertyChanged 对称：折叠/展开后重建一次平铺行，
            // 而不是靠行模板里的嵌套 Expander（嵌套会被赋予无限高度，虚拟化失效）。
            RebuildRowsFromCategories();
        }

        /// <summary>按各分类的展开状态压平成单层行（折叠的分类只保留分类头）。
        /// 压平是虚拟化的前提：所有行在同一个面板里、行高一致，
        /// 嵌套结构（Expander 内套 ItemsControl）会被赋予无限高度，虚拟化等于没开。</summary>
        private void RebuildRowsFromCategories()
        {
            var rows = new List<object>(EventCategories.Count + FilteredEvents.Count);
            foreach (var c in EventCategories)
            {
                rows.Add(c);
                if (!c.IsExpanded) continue;

                // 标记组内首/末项：行模板据此画卡片的上/下边框与圆角，
                // 与属性页的分类卡片保持同一套观感。
                for (var i = 0; i < c.Items.Count; i++)
                {
                    var item = c.Items[i];
                    item.IsGroupFirst = i == 0;
                    item.IsGroupLast = i == c.Items.Count - 1;
                    rows.Add(item);
                }
            }
            RebuildRows(rows);
        }

        // ---------------- 行重建（虚拟化 + 滚动位置保持） ----------------

        /// <summary>把平铺行赋给列表。与 PropertyGridPro.RebuildRows 同款手法：
        /// ① 预热（先 1 行再全量）只用于 offset=0 的首次填充，修 CanContentScroll 下
        /// VirtualizingStackPanel 首次布局把全部行算进视口导致的无滚动条；
        /// ② 已滚动的重建绝不预热（1 行中间态会扰动面板内部偏移），且恢复偏移要
        /// 同步拨一次 + Loaded 优先级再拨一次（面板会在后续布局里覆盖同步那次）。</summary>
        private void RebuildRows(List<object> rows)
        {
            if (_itemsControl == null) return;

            // 惰性获取：PART_ScrollViewer 在 PART_ItemsControl 自己的 ControlTemplate 内
            // （与基类/Pro 同款），OnApplyTemplate 时内层模板尚未实例化拿不到，布局后这里才拿得到
            if (_scrollViewer == null)
                _scrollViewer = _itemsControl.Template?.FindName("PART_ScrollViewer", _itemsControl) as ScrollViewer;

            double offset = _scrollViewer?.VerticalOffset ?? 0;

            if (offset <= 0 && rows.Count > 0)
            {
                _itemsControl.ItemsSource = new object[] { rows[0] };
                _scrollViewer?.UpdateLayout();
            }
            _itemsControl.ItemsSource = rows;

            if (offset > 0)
            {
                _scrollViewer?.UpdateLayout();
                _scrollViewer?.ScrollToVerticalOffset(offset);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _scrollViewer?.ScrollToVerticalOffset(offset);
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        // ---------------- 编辑回调 ----------------

        /// <summary>请求编辑事件处理代码（双击事件行或点击 ⚡ 触发；宿主应打开代码编辑器）。
        /// 无订阅者时执行默认行为：为未绑定的事件填入建议方法名。</summary>
        public event EventHandler<EventItemEventArgs> EditRequested;

        /// <summary>单击事件行 → 记录为选中事件，供底部描述栏显示（不置 Handled：
        /// 不影响点 ⚡ 按钮、也不影响行内输入框正常取焦点）。</summary>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var item = FindEventItem(e.OriginalSource as DependencyObject);
            if (item != null) SelectedEvent = item;
        }

        /// <summary>双击事件行触发编辑请求</summary>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            var item = FindEventItem(e.OriginalSource as DependencyObject);
            if (item != null)
            {
                RaiseEdit(item);
                e.Handled = true;
            }
        }

        internal void RaiseEdit(EventItem item)
        {
            if (item == null) return;
            if (EditRequested != null)
            {
                EditRequested(this, new EventItemEventArgs(item));
            }
            else
            {
                // 无宿主接管 → EventItem 默认行为（填建议方法名）
                item.EditCommand?.Execute(null);
            }
        }

        internal static EventItem FindEventItem(DependencyObject source)
        {
            while (source != null)
            {
                if (source is FrameworkElement fe && fe.DataContext is EventItem ei) return ei;
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);
            }
            return null;
        }

        /// <summary>默认建议方法名：控件名_事件名（无控件名时用 "On"+事件名）</summary>
        public static string SuggestHandlerName(EventItem item, string controlName = null)
        {
            var prefix = string.IsNullOrWhiteSpace(controlName) ? "On" : controlName + "_";
            return prefix + item.Name;
        }
    }

    /// <summary>事件分类分组（EventPicker 平铺行的分类头）。
    /// 与属性页的 PropertyCategory 同构：带 IsExpanded 并可通知，
    /// 供分类头折叠/展开（折叠的分类只保留分类头一行）。</summary>
    public class EventCategory : INotifyPropertyChanged
    {
        /// <summary>分类名</summary>
        public string Name { get; }

        /// <summary>该分类下的事件项</summary>
        public List<EventItem> Items { get; }

        public EventCategory(string name, List<EventItem> items)
        {
            Name = name;
            Items = items ?? new List<EventItem>();
            _isExpanded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isExpanded;
        /// <summary>是否展开（默认展开，与属性页分类头一致；折叠后该类事件行不进入平铺列表）</summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }
    }

    /// <summary>
    /// EventPicker 平铺行的模板选择器：分类头行用分类头模板，事件行用行模板。
    /// 刻意不用隐式 DataTemplate（DataType=EventItem）——隐式模板对后代元素同样生效，
    /// 行内编辑器一旦解析不到模板会回退到自身导致无限递归（与 ProFlatRowTemplateSelector 同因）。
    /// </summary>
    public class EventFlatRowTemplateSelector : DataTemplateSelector
    {
        /// <summary>分类头模板（平铺列表中的分组标签行）</summary>
        public DataTemplate CategoryTemplate { get; set; }

        /// <summary>事件行模板</summary>
        public DataTemplate ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
            => item is EventCategory ? CategoryTemplate : ItemTemplate;
    }
}
