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
        private bool _isDarkMode;
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
            _isDarkMode = !_isDarkMode;
            SetAppSkin(_isDarkMode ? HandyControl.Data.SkinType.Dark : HandyControl.Data.SkinType.Default);
        }

        private static void SetAppSkin(HandyControl.Data.SkinType skin)
        {
            var appRes = Application.Current.Resources;
            HandyControl.Themes.Theme theme = null;
            for (int i = 0; i < appRes.MergedDictionaries.Count; i++)
            {
                if (appRes.MergedDictionaries[i] is HandyControl.Themes.Theme t)
                {
                    theme = t;
                    break;
                }
            }

            if (theme == null)
            {
                theme = new HandyControl.Themes.Theme();
                appRes.MergedDictionaries.Add(theme);
            }

            theme.Skin = skin;
        }

        private void SwitchObject_Click(object sender, RoutedEventArgs e)
        {
            _useSecondObject = !_useSecondObject;
            if (_useSecondObject)
            {
                propertyGrid.SelectedObject = new SampleObject();
            }
            else
            {
                propertyGrid.SelectedObject = new SampleObject2();
            }
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
