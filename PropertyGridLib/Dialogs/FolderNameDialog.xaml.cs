using System.IO;
using System.Windows;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Dialogs
{
    /// <summary>
    /// 文件夹名称输入对话框（新建文件夹 / 重命名共用）
    /// </summary>
    public partial class FolderNameDialog
    {
        public FolderNameDialog()
        {
            // 先于 InitializeComponent 赋字体
            FontFamily = PropertyGrid.GlobalFontFamily;
            FontSize = PropertyGrid.GlobalFontSize;

            InitializeComponent();
            UiScaler.Attach(this);

            Loaded += (_, __) =>
            {
                NameTextBox.Focus();
                NameTextBox.SelectAll();
            };
        }

        /// <summary>用户输入的名称（已去首尾空白）</summary>
        public string FolderName => NameTextBox.Text.Trim();

        /// <summary>
        /// 弹出名称输入框；确认返回名称，取消返回 null
        /// </summary>
        public static string? Prompt(Window owner, string title, string prompt, string initialName)
        {
            var dlg = new FolderNameDialog { Title = title, Owner = owner };
            dlg.PromptText.Text = prompt;
            dlg.NameTextBox.Text = initialName;
            return dlg.ShowDialog() == true ? dlg.FolderName : null;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var name = FolderName;
            if (string.IsNullOrWhiteSpace(name))
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("FolderNameEmpty"),
                    LocalizationManager.GetString("Tip"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                NameTextBox.Focus();
                return;
            }

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("InvalidFolderName"),
                    LocalizationManager.GetString("Tip"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                NameTextBox.Focus();
                return;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
