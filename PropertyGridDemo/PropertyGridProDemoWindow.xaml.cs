using System.Windows;
using PropertyGridLib.Controls;

namespace PropertyGridDemo
{
    /// <summary>
    /// PropertyGridPro 演示窗口：跟随主题的「参数配置」平铺面板。
    /// 反射、属性项与编辑器全部复用 PropertyGridLib，Demo 侧只提供数据模型与交互；
    /// 窗口自身所有配色均取自主题资源键，点击"切换主题"即可在浅色 / 深色间对比。
    /// </summary>
    public partial class PropertyGridProDemoWindow : Window
    {
        public PropertyGridProDemoWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateThemeLabel();
            proGrid.SelectedObject = new Models.SampleObject();
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeHelper.Toggle();
            UpdateThemeLabel();
        }

        private void UpdateThemeLabel()
        {
            if (txtTheme != null)
            {
                txtTheme.Text = ThemeHelper.IsDark ? "当前主题：深色" : "当前主题：浅色";
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            proGrid.RefreshProperties();
        }

        private void ResetAll_Click(object sender, RoutedEventArgs e)
        {
            proGrid.ResetAllToDefault();
        }

        private void ProGrid_PropertyValueChanged(object sender, System.EventArgs e)
        {
            // 属性值变化（宿主可在此写回自己的模型，演示窗无需处理）
        }

        private void ProGrid_ButtonClicked(object sender, PropertyButtonClickEventArgs e)
        {
            // [Button] 点击（演示窗无需处理）
        }
    }
}
