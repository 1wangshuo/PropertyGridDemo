using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 路径选择控件：单行文本框 + 「...」浏览按钮，可脱离 PropertyGrid 独立使用（SDK）。
    /// - <see cref="SelectDirectory"/>=false：打开文件选择对话框（<see cref="Filter"/> 过滤、<see cref="Title"/> 标题）；
    /// - <see cref="SelectDirectory"/>=true：打开目录选择对话框（标题取 <see cref="Title"/>，缺省走本地化「选择文件夹」）；
    /// - <see cref="Text"/> 双向绑定路径值；<see cref="IsReadOnly"/> 时文本框只读且浏览按钮禁用。
    /// 属性网格的 [FilePath]/[DirectoryPath] 编辑器模板内部即此控件。
    /// </summary>
    public class FilePathControl : Control
    {
        static FilePathControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilePathControl),
                new FrameworkPropertyMetadata(typeof(FilePathControl)));
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_BrowseButton") is Button browse)
            {
                browse.Click -= BrowseButton_Click;
                browse.Click += BrowseButton_Click;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // 宿主替换了 BrowseCommand 则交给宿主；否则执行内置对话框
            if (BrowseCommand is ICommand c)
            {
                if (c.CanExecute(null)) c.Execute(null);
                return;
            }
            Browse();
        }

        // ---------------- 路径值 ----------------

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(FilePathControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>路径值（双向）</summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // ---------------- 对话框配置 ----------------

        /// <summary>文件过滤器（如 "*.log|*.json"；null = 所有文件）</summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FilePathControl),
                new PropertyMetadata(null));

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        /// <summary>对话框标题；null = 库内置本地化文字（随语言）</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(FilePathControl),
                new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>true = 选择目录（FolderBrowser）；false = 选择文件（OpenFileDialog）</summary>
        public static readonly DependencyProperty SelectDirectoryProperty =
            DependencyProperty.Register(nameof(SelectDirectory), typeof(bool), typeof(FilePathControl),
                new PropertyMetadata(false));

        public bool SelectDirectory
        {
            get => (bool)GetValue(SelectDirectoryProperty);
            set => SetValue(SelectDirectoryProperty, value);
        }

        /// <summary>只读：文本框只读 + 浏览按钮禁用</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(FilePathControl),
                new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        // ---------------- 浏览 ----------------

        /// <summary>「...」按钮点击（模板内 PART_BrowseButton 绑定此命令）</summary>
        public static readonly DependencyProperty BrowseCommandProperty =
            DependencyProperty.Register(nameof(BrowseCommand), typeof(ICommand), typeof(FilePathControl),
                new PropertyMetadata(null));

        /// <summary>浏览命令（模板绑定；宿主可替换自定义浏览逻辑，null 时执行内置对话框）</summary>
        public ICommand BrowseCommand
        {
            get => (ICommand)GetValue(BrowseCommandProperty);
            set => SetValue(BrowseCommandProperty, value);
        }

        /// <summary>内置浏览逻辑：按 SelectDirectory 打开目录/文件对话框，确认后写回 Text</summary>
        public void Browse()
        {
            if (SelectDirectory)
            {
                var dlg = new Dialogs.FolderBrowserDialog
                {
                    SelectedPath = Text
                };
                if (!string.IsNullOrEmpty(Title))
                    dlg.Title = Title;
                if (dlg.ShowDialog(Application.Current?.MainWindow) == true)
                    Text = dlg.SelectedPath;
                return;
            }

            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Title = string.IsNullOrEmpty(Title) ? LocalizationManager.GetString("SelectFile") : Title
            };
            var filter = Filter;
            ofd.Filter = string.IsNullOrEmpty(filter) == false
                ? $"{LocalizationManager.GetString("Files")}|{filter}|{LocalizationManager.GetString("AllFiles")}|*.*"
                : $"{LocalizationManager.GetString("AllFiles")}|*.*";
            // 初始目录 = 当前路径所在目录（有值且存在时）
            try
            {
                var dir = System.IO.Path.GetDirectoryName(Text);
                if (string.IsNullOrEmpty(dir) == false && System.IO.Directory.Exists(dir))
                    ofd.InitialDirectory = dir;
            }
            catch { /* 非法路径字符等：忽略初始目录 */ }
            if (ofd.ShowDialog(Application.Current?.MainWindow) == true)
                Text = ofd.FileName;
        }
    }
}
