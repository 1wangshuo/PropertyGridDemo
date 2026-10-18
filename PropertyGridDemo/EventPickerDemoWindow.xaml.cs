using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PropertyGridLib.Controls;

namespace PropertyGridDemo
{
    public partial class EventPickerDemoWindow : Window
    {
        private Button _sourceButton;

        /// <summary>若干常见 WPF 事件的说明。反射本身取不到 [Description]，
        /// 这里补一份便于观察底部描述栏（只有带说明的事件行被选中时才出现）。</summary>
        private static readonly Dictionary<string, string> EventDescriptions = new Dictionary<string, string>
        {
            ["Click"] = "按钮被点击（按下并释放）时触发，最常用的命令入口，通常在此执行主业务逻辑。",
            ["MouseDoubleClick"] = "鼠标左键在按钮上双击时触发，用于把双击与单击两种交互区分开。",
            ["GotFocus"] = "按钮获得键盘焦点时触发，可在此做选中态提示（如高亮边框）。",
            ["PreviewMouseDown"] = "隧道事件：鼠标按下先于 Click 触发，适合做按下前的干预（拦截/禁用）。",
            ["DragEnter"] = "拖拽对象进入按钮范围时触发，常配合 Drop 实现拖放接收。",
            ["Drop"] = "拖拽在按钮上释放时触发，在此读取拖拽数据完成接收。"
        };

        public EventPickerDemoWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _sourceButton = new Button { Content = "目标按钮" };
                LoadEvents();
            };
        }

        /// <summary>反射按钮的全部公共事件，并给若干常见事件补上说明</summary>
        private void LoadEvents()
        {
            EventPicker.Events = EventItemFactory.CreateEvents(_sourceButton)
                .Select(ev => new EventItem(
                    ev.Name,
                    ev.EventType,
                    ev.HandlerName,
                    ev.Category,
                    EventDescriptions.TryGetValue(ev.Name, out var desc) ? desc : null,
                    ev.DisplayName))
                .ToList();
        }

        private void OnSourceButtonClick(object sender, RoutedEventArgs e)
        {
            _sourceButton.Content = $"被点击了 {++_clickCount} 次";
            LoadEvents(); // 值变化后可重刷（事件集合不变时无感知），说明映射一并保留
        }

        private int _clickCount;

        private void OnEventEditRequested(object sender, EventItemEventArgs e)
        {
            MessageBox.Show(this,
                $"事件编辑请求：\n事件：{e.Item.DisplayName}\n名称：{e.Item.Name}\n" +
                (string.IsNullOrEmpty(e.Item.HandlerName) ? "（尚未绑定处理方法）" : $"处理方法：{e.Item.HandlerName}"),
                "EventPicker EditRequested", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
