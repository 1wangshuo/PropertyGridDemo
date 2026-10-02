using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Dialogs
{
    /// <summary>
    /// 自定义文件夹浏览对话框，替代 System.Windows.Forms.FolderBrowserDialog
    /// </summary>
    public partial class FolderBrowserDialog : INotifyPropertyChanged
    {
        private string _selectedPath = string.Empty;
        private string _description = string.Empty;
        private string _title = string.Empty;
        private bool _isSyncing; // 防止 TextBox.Text 程序赋值触发 TextChanged 重入

        public event PropertyChangedEventHandler? PropertyChanged;

        public FolderBrowserDialog()
        {
            // 先于 InitializeComponent 赋字体
            this.FontFamily = PropertyGrid.GlobalFontFamily;
            this.FontSize = PropertyGrid.GlobalFontSize;
            
            InitializeComponent();
            UiScaler.Attach(this);
            
            DataContext = this;
            Title = LocalizationManager.GetString("FolderBrowserTitle");
            Description = LocalizationManager.GetString("FolderBrowserTitle");
            
            LoadDrives();
            
            // 订阅语言切换事件
            LocalizationManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            Title = LocalizationManager.GetString("FolderBrowserTitle");
            if (string.IsNullOrEmpty(Description) || 
                Description == LocalizationManager.GetString("FolderBrowserTitle"))
            {
                Description = LocalizationManager.GetString("FolderBrowserTitle");
            }
        }

        /// <summary>
        /// 对话框标题
        /// </summary>
        public new string Title
        {
            get => _title;
            set
            {
                _title = value;
                base.Title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// 描述文本
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        /// <summary>
        /// 选中的路径
        /// </summary>
        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                if (_selectedPath == value) return;
                _selectedPath = value;
                OnPropertyChanged(nameof(SelectedPath));

                // 程序赋值时不触发 TextChanged（避免重入），仅更新文本框
                _isSyncing = true;
                SelectedPathTextBox.Text = value;
                _isSyncing = false;
            }
        }

        /// <summary>
        /// 显示对话框
        /// </summary>
        public bool? ShowDialog(Window owner)
        {
            Owner = owner;

            // 显示前把预设路径在树上定位好
            if (!string.IsNullOrEmpty(SelectedPath))
                ExpandToPath(SelectedPath);

            return ShowDialog();
        }

        private void LoadDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => new FolderItem(d.RootDirectory.FullName))
                    .ToList();

                foreach (var drive in drives)
                {
                    FolderTreeView.Items.Add(drive);
                }

                // 如果有预设路径，尝试展开到该路径
                if (!string.IsNullOrEmpty(SelectedPath) && Directory.Exists(SelectedPath))
                {
                    ExpandToPath(SelectedPath);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("LoadDrivesFailed", ex.Message),
                    LocalizationManager.GetString("Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExpandToPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path.Trim().Trim('"'));
            }
            catch
            {
                return; // 非法路径，忽略
            }

            // 只有目录真实存在才在树上定位，避免用户输入不存在的路径时反复刷新
            if (!Directory.Exists(fullPath))
                return;

            // 生成从根到目标的各级完整路径（根 → 目标）
            var segments = new List<string>();
            var current = fullPath;
            while (true)
            {
                segments.Add(current);
                var parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent) || parent == current)
                    break;
                current = parent;
            }
            segments.Reverse();

            // 找到与根路径匹配的顶级 TreeView 节点（一般是驱动器，如 "C:\"）
            FolderItem node = null;
            foreach (var obj in FolderTreeView.Items)
            {
                if (obj is FolderItem fi &&
                    string.Equals(NormalizePath(fi.FullPath), NormalizePath(segments[0]), StringComparison.OrdinalIgnoreCase))
                {
                    node = fi;
                    break;
                }
            }

            if (node == null)
                return;

            // 逐级下钻；任何一级在树中找不到就停止（符合"除非没有这一层"）
            for (int i = 1; i < segments.Count; i++)
            {
                var seg = segments[i];
                FolderItem child = null;
                foreach (var sub in node.SubFolders)
                {
                    if (string.Equals(NormalizePath(sub.FullPath), NormalizePath(seg), StringComparison.OrdinalIgnoreCase))
                    {
                        child = sub;
                        break;
                    }
                }
                if (child == null)
                    return; // 树中不存在这一层，停止

                node.IsExpanded = true; // 展开当前父级，露出子级
                node = child;
            }

            // 到达目标：展开并选中
            node.IsExpanded = true;
            node.IsSelected = true;
            ScrollToItem(node);
        }

        /// <summary>规范化路径用于比较（去掉尾部斜杠/反斜杠）</summary>
        private static string NormalizePath(string p)
        {
            if (string.IsNullOrEmpty(p)) return string.Empty;
            return p.TrimEnd('\\', '/');
        }

        /// <summary>让目标节点滚动到可视区域并聚焦</summary>
        private void ScrollToItem(FolderItem item)
        {
            // 延迟到布局完成后，确保虚拟化容器已生成
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    FolderTreeView.UpdateLayout();
                    var container = FolderTreeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    container?.BringIntoView();
                    container?.Focus();
                }
                catch
                {
                    // 滚动失败不影响选中，忽略
                }
            }), DispatcherPriority.Background);
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FolderItem folderItem)
            {
                SelectedPath = folderItem.FullPath;
            }
        }

        private void SelectedPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 防止程序赋值（SelectedPath setter）触发重入
            if (_isSyncing) return;

            _selectedPath = SelectedPathTextBox.Text;
            OnPropertyChanged(nameof(SelectedPath));

            // 手动输入合法路径后，自动在树上展开并选中对应文件夹
            ExpandToPath(_selectedPath);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证路径
            if (string.IsNullOrWhiteSpace(SelectedPath))
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("PleaseSelectFolder"),
                    LocalizationManager.GetString("Tip"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!Directory.Exists(SelectedPath))
            {
                var result = HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("FolderNotExistCreateIt"),
                    LocalizationManager.GetString("Tip"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(SelectedPath);
                    }
                    catch (Exception ex)
                    {
                        HandyControl.Controls.MessageBox.Show(
                            LocalizationManager.GetString("CreateFolderFailed", ex.Message),
                            LocalizationManager.GetString("Error"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 文件夹项模型
    /// </summary>
    public class FolderItem : INotifyPropertyChanged
    {
        private ObservableCollection<FolderItem> _subFolders;
        private bool _isLoaded;

        public event PropertyChangedEventHandler PropertyChanged;

        public FolderItem(string fullPath)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(Name))
            {
                Name = fullPath; // 对于驱动器根目录
            }
        }

        public string Name { get; }
        public string FullPath { get; }

        private bool _isExpanded;
        private bool _isSelected;

        /// <summary>是否展开（绑定到 TreeViewItem.IsExpanded）</summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        /// <summary>是否选中（绑定到 TreeViewItem.IsSelected）</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public ObservableCollection<FolderItem> SubFolders
        {
            get
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;
                    _subFolders = LoadSubFolders();
                }
                return _subFolders;
            }
        }

        private ObservableCollection<FolderItem> LoadSubFolders()
        {
            var folders = new ObservableCollection<FolderItem>();
            try
            {
                var dirs = Directory.GetDirectories(FullPath);
                foreach (var dir in dirs)
                {
                    try
                    {
                        // 跳过隐藏和系统文件夹
                        var dirInfo = new DirectoryInfo(dir);
                        if ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                            (dirInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                        {
                            continue;
                        }

                        folders.Add(new FolderItem(dir));
                    }
                    catch
                    {
                        // 忽略无权访问的文件夹
                    }
                }
            }
            catch
            {
                // 忽略访问错误
            }

            return folders;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
