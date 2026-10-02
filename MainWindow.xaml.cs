using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using HandyControl.Tools;
using PropertyGridLib;
using PropertyGridLib.Localization;
using PropertyGridDemo.Models;

namespace PropertyGridDemo
{
    public partial class MainWindow : Window
    {
        private bool _useSecondObject;
        private bool _isEnglish;

        public MainWindow()
        {
            InitializeComponent();

            PropertyGridLib.PropertyGrid.RegisterEditor<DemoTimeSpanEditorAttribute>(
                (DataTemplate)FindResource("DemoTimeSpanEditorTemplate"));

            WindowState = WindowState.Maximized;
            Loaded += (s, e) => UpdatePropertyValues();
            InitAppearancePanel();

            // 压测开关（Demo 专用）：--stress-pro / --stress-pro-novirt
            // 自动打开 PropertyGridPro 窗口并跑刷新压测，结果写入 exe 同目录 pro-stress-result.txt
            if (Environment.GetCommandLineArgs()
                .Any(a => a.StartsWith("--stress-pro", StringComparison.OrdinalIgnoreCase)))
            {
                Loaded += (s, e) => new PropertyGridProDemoWindow().Show();
            }

            PropertyGrid.GlobalButtonClicked += OnPropertyGridButtonClicked;
            Closed += (s, e) => PropertyGrid.GlobalButtonClicked -= OnPropertyGridButtonClicked;
        }

        private void OnPropertyGridButtonClicked(object sender, PropertyGridLib.Controls.PropertyButtonClickEventArgs e)
        {
            if (e.PropertyName == nameof(SampleObject.ExportButton))
            {
                MessageBox.Show(this,
                    $"导出配置：按钮 \"{e.PropertyItem.DisplayName}\" 被点击了。\n该按钮未指定 Action，仅通过全局 ButtonClicked 事件通知宿主。",
                    "按钮点击事件", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            UpdatePropertyValues();
        }

        private void ResetSelected_Click(object sender, RoutedEventArgs e)
        {
            propertyGrid.ResetSelectedToDefault();
            UpdatePropertyValues();
        }

        private void ResetAll_Click(object sender, RoutedEventArgs e)
        {
            propertyGrid.ResetAllToDefault();
            UpdatePropertyValues();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            propertyGrid.RefreshProperties();
            UpdatePropertyValues();
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeHelper.Toggle();
        }

        private void SwitchObject_Click(object sender, RoutedEventArgs e)
        {
            _useSecondObject = !_useSecondObject;
            propertyGrid.SelectedObject = _useSecondObject ? new SampleObject2() : new SampleObject();
            UpdatePropertyValues();
        }

        private void ToggleLanguage_Click(object sender, RoutedEventArgs e)
        {
            _isEnglish = !_isEnglish;
            LocalizationManager.CurrentLanguage = _isEnglish ? PropertyGridLib.Localization.Language.EnUS : PropertyGridLib.Localization.Language.ZhCN;
        }

        private void CollectionEditorDemo_Click(object sender, RoutedEventArgs e)
        {
            var win = new CollectionEditorDemoWindow
            {
                Owner = this
            };
            win.ShowDialog();
        }

        private void PropertyGridProDemo_Click(object sender, RoutedEventArgs e)
        {
            var win = new PropertyGridProDemoWindow
            {
                Owner = this
            };
            win.ShowDialog();
        }

        private bool _smallLayout;

        /// <summary>Small 紧凑布局切换：TitlePlacement Top（默认/标题在编辑器上方）↔ Left（Small/标题在编辑器左侧，行高压缩近一半）</summary>
        private void ToggleSmallLayout_Click(object sender, RoutedEventArgs e)
        {
            _smallLayout = !_smallLayout;
            propertyGrid.TitlePlacement = _smallLayout
                ? PropertyGridLib.PropertyGridTitlePlacement.Left
                : PropertyGridLib.PropertyGridTitlePlacement.Top;
            SmallLayoutButton.Content = _smallLayout ? "恢复默认布局" : "Small 紧凑布局";
            UpdatePropertyValues();
        }

        private void EventPickerDemo_Click(object sender, RoutedEventArgs e)
        {
            var win = new EventPickerDemoWindow
            {
                Owner = this
            };
            win.ShowDialog();
        }

        private void InitAppearancePanel()
        {
            // 字体列表
            foreach (var family in System.Windows.Media.Fonts.SystemFontFamilies)
                fontFamilyCombo.Items.Add(family);
            fontFamilyCombo.SelectedItem = new System.Windows.Media.FontFamily("Microsoft YaHei UI");

            // 字号列表
            var sizes = new double[] { 10, 11, 12, 13, 14, 15, 16, 18, 20, 24 };
            foreach (var s in sizes)
                fontSizeCombo.Items.Add(s);
            fontSizeCombo.SelectedItem = 14.0;
        }

        private void FontFamily_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (fontFamilyCombo.SelectedItem is System.Windows.Media.FontFamily family)
                propertyGrid.FontFamily = family;
        }

        private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (fontSizeCombo.SelectedItem is double size)
                propertyGrid.FontSize = size;
        }

        /// <summary>
        /// 显示当前对象的属性值
        /// </summary>
        private void UpdatePropertyValues()
        {
            var obj = propertyGrid.SelectedObject;
            if (obj == null)
            {
                propertyValuesText.Text = "(无选中对象)";
                return;
            }

            var sb = new StringBuilder();
            var props = obj.GetType().GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.Name);

            foreach (var prop in props)
            {
                try
                {
                    var val = prop.GetValue(obj);
                    var display = val?.ToString() ?? "(null)";
                    if (display.Length > 40) display = display.Substring(0, 40) + "...";
                    sb.AppendLine($"  {prop.Name,-22} = {display}");
                }
                catch
                {
                    sb.AppendLine($"  {prop.Name,-22} = [读取失败]");
                }
            }
            propertyValuesText.Text = sb.ToString();
        }
    }
}
