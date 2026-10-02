using System.Windows;
using System.Windows.Controls;
using PropertyGridLib.Controls;

namespace PropertyGridDemo
{
    public partial class EventPickerDemoWindow : Window
    {
        private Button _sourceButton;

        public EventPickerDemoWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _sourceButton = new Button { Content = "目标按钮" };
                // 让 EventPicker 直接反射按钮的全部公共事件
                EventPicker.SelectedObject = _sourceButton;
            };
        }

        private void OnSourceButtonClick(object sender, RoutedEventArgs e)
        {
            _sourceButton.Content = $"被点击了 {++_clickCount} 次";
            EventPicker.SelectedObject = _sourceButton; // 值变化后可重刷（事件集合不变时无感知）
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
