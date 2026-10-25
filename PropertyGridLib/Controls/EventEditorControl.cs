using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 事件行编辑器（独立控件，PropertyGridLib 1.4.0 新增，1.8 起支持脱离 PropertyGrid 使用）：
    /// 处理方法名文本框（LostFocus/回车提交）+ ⚡ 按钮（发起编辑请求）。
    /// 编辑请求以路由事件冒泡，EventPicker / PropertyGrid / 宿主任一层均可接管。
    ///
    /// 两种用法：
    /// ① 网格内联：DataContext 为 <see cref="EventItem"/>，DP 与其 HandlerName/IsReadOnly 自动同步；
    /// ② 独立使用：直接给 <see cref="HandlerName"/> / <see cref="IsReadOnly"/> / <see cref="Placeholder"/>
    ///    / <see cref="IsBound"/> 赋值或绑定，无需 PropertyGrid/EventItem。
    /// </summary>
    public class EventEditorControl : Control
    {
        static EventEditorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EventEditorControl),
                new FrameworkPropertyMetadata(typeof(EventEditorControl)));
        }

        /// <summary>构造：订阅 DataContextChanged（net48 无 OnDataContextChanged 虚方法）</summary>
        public EventEditorControl()
        {
            DataContextChanged += (s, e) => SyncFromItem();

            // 占位文字默认值走本地化资源。不能写在模板绑定的 FallbackValue 上 ——
            // WPF 禁止在 Binding 的 FallbackValue 里再套 MarkupExtension（XamlParseException 闪退）。
            if (Placeholder == null)
            {
                var localized = LocalizationManager.GetString("NoHandler");
                SetCurrentValue(PlaceholderProperty,
                    string.IsNullOrEmpty(localized) ? "未绑定" : localized);
            }
        }

        private TextBox _nameBox;

        /// <summary>请求编辑事件代码（路由事件，参数携带对应 <see cref="EventItem"/>；独立用法下 Item 可能为 null）</summary>
        public static readonly RoutedEvent EditRequestedEvent = EventManager.RegisterRoutedEvent(
            "EditRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EventEditorControl));

        /// <summary>请求编辑事件代码（沿可视化树冒泡，宿主可接管；接管的层请置 Handled=true）</summary>
        public event RoutedEventHandler EditRequested
        {
            add { AddHandler(EditRequestedEvent, value); }
            remove { RemoveHandler(EditRequestedEvent, value); }
        }

        // ---------------- 独立使用（不依赖 EventItem） ----------------

        /// <summary>处理方法名。默认双向绑定；DataContext 为 EventItem 时与它的 HandlerName 双向同步。</summary>
        public static readonly DependencyProperty HandlerNameProperty =
            DependencyProperty.Register(nameof(HandlerName), typeof(string), typeof(EventEditorControl),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHandlerNameChanged));

        /// <summary>处理方法名</summary>
        public string HandlerName
        {
            get => (string)GetValue(HandlerNameProperty);
            set => SetValue(HandlerNameProperty, value);
        }

        /// <summary>是否已绑定处理方法（独立用法下可用于绿标/自定义提示，本控件模板不直接渲染它）</summary>
        public static readonly DependencyProperty IsBoundProperty =
            DependencyProperty.Register(nameof(IsBound), typeof(bool), typeof(EventEditorControl),
                new PropertyMetadata(false));

        /// <summary>是否已绑定处理方法</summary>
        public bool IsBound
        {
            get => (bool)GetValue(IsBoundProperty);
            set => SetValue(IsBoundProperty, value);
        }

        /// <summary>只读（独立用法直接设置；网格内联时同步自 EventItem）</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(EventEditorControl),
                new PropertyMetadata(false));

        /// <summary>只读</summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>输入框占位文字。为 null 时模板回落到本地化的「未绑定」（见 Generic.xaml FallbackValue）。</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(EventEditorControl),
                new PropertyMetadata(null));

        /// <summary>输入框占位文字</summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>DP ↔ EventItem 双向同步中，防止回环</summary>
        private bool _syncing;

        private static void OnHandlerNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is EventEditorControl ctl)) return;
            // 独立用法（DataContext 不是 EventItem）不写回
            if (ctl._syncing || !(ctl.DataContext is EventItem item)) return;
            ctl._syncing = true;
            try { item.HandlerName = e.NewValue as string; }
            finally { ctl._syncing = false; }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _nameBox = GetTemplateChild("PART_HandlerNameBox") as TextBox;
            var editButton = GetTemplateChild("PART_EditButton") as Button;
            if (editButton != null)
                editButton.Click += (s, e) => RequestEdit();
            if (_nameBox != null)
            {
                _nameBox.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        _nameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                        e.Handled = true;
                    }
                };
            }
            SyncFromItem();
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            // net48 兼容：DataContext 变化时同步（DataContextChanged 事件已订阅，这里兜底）
            base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty)
                SyncFromItem();
        }

        /// <summary>网格内联：把 EventItem 的值灌进 DP（模板只认 DP，不认 DataContext，独立用法才能成立）</summary>
        private void SyncFromItem()
        {
            if (!(DataContext is EventItem item))
            {
                // 独立用法：输入框内容以 HandlerName 为准（模板已绑 DP，无需额外处理）
                return;
            }

            _syncing = true;
            try
            {
                SetCurrentValue(HandlerNameProperty, item.HandlerName ?? string.Empty);
                SetCurrentValue(IsBoundProperty, item.HasHandler);
                SetCurrentValue(IsReadOnlyProperty, item.IsReadOnly);
            }
            finally { _syncing = false; }
        }

        /// <summary>⚡ 按钮入口：冒泡编辑请求；无宿主接管（Handled=false）时执行 EventItem 默认行为</summary>
        public void RequestEdit()
        {
            _nameBox?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            var item = DataContext as EventItem;
            var args = new EventEditRoutedEventArgs(EditRequestedEvent, item);
            RaiseEvent(args);
            if (!args.Handled && item != null)
            {
                // 没有任何宿主接管 → EventItem 默认行为（填入建议方法名）
                item.EditCommand?.Execute(null);
                SyncFromItem();
            }
        }
    }

    /// <summary>事件编辑请求的路由事件参数（携带对应 EventItem；独立用法下为 null）</summary>
    public class EventEditRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>请求编辑的事件项（独立控件用法下为 null，宿主可从发送方读取 HandlerName）</summary>
        public EventItem Item { get; }

        public EventEditRoutedEventArgs(RoutedEvent routedEvent, EventItem item)
            : base(routedEvent)
        {
            Item = item;
        }
    }
}
