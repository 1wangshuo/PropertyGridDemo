using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 事件页行内编辑器（PropertyGridLib 1.4.0 新增）：
    /// 处理方法名文本框（LostFocus/回车提交）+ ⚡ 按钮（发起编辑请求）。
    /// 编辑请求以路由事件冒泡，EventPicker / PropertyGrid / 宿主任一层均可接管。
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
        }

        private TextBox _nameBox;

        /// <summary>请求编辑事件代码（路由事件，参数携带对应 <see cref="EventItem"/>）</summary>
        public static readonly RoutedEvent EditRequestedEvent = EventManager.RegisterRoutedEvent(
            "EditRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EventEditorControl));

        /// <summary>请求编辑事件代码（沿可视化树冒泡，宿主可接管；接管的层请置 Handled=true）</summary>
        public event RoutedEventHandler EditRequested
        {
            add { AddHandler(EditRequestedEvent, value); }
            remove { RemoveHandler(EditRequestedEvent, value); }
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

        private void SyncFromItem()
        {
            if (_nameBox == null || !(DataContext is EventItem item)) return;
            var text = item.HandlerName ?? string.Empty;
            if (!string.Equals(_nameBox.Text, text, StringComparison.Ordinal))
                _nameBox.Text = text;
        }

        /// <summary>⚡ 按钮入口：冒泡编辑请求；无宿主接管（Handled=false）时执行 EventItem 默认行为</summary>
        public void RequestEdit()
        {
            if (DataContext is EventItem item)
            {
                _nameBox?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                var args = new EventEditRoutedEventArgs(EditRequestedEvent, item);
                RaiseEvent(args);
                if (!args.Handled)
                {
                    // 没有任何宿主接管 → EventItem 默认行为（填入建议方法名）
                    item.EditCommand?.Execute(null);
                }
                SyncFromItem();
            }
        }
    }

    /// <summary>事件编辑请求的路由事件参数（携带对应 EventItem）</summary>
    public class EventEditRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>请求编辑的事件项</summary>
        public EventItem Item { get; }

        public EventEditRoutedEventArgs(RoutedEvent routedEvent, EventItem item)
            : base(routedEvent)
        {
            Item = item;
        }
    }
}
