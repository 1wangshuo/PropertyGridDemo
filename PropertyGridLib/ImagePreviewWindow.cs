using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PropertyGridLib
{
    /// <summary>
    /// 属性面板的"大图预览"窗口：属性行里的缩略图点开就是它。
    ///
    /// 基类用 HandyControl 的 <see cref="HandyControl.Controls.Window"/>：
    /// 原生 WPF 窗口的非客户区（标题栏/边框）由系统绘制，深色皮肤下会是一块系统灰，
    /// 和面板对不上；hc:Window 自己画标题栏并用主题画刷上色，切皮肤时自动跟随。
    ///
    /// 内容侧仍然只用主题键（RegionBrush / BorderBrush / PrimaryTextBrush / SecondaryTextBrush），
    /// 不写死任何颜色。
    /// </summary>
    public sealed class ImagePreviewWindow : HandyControl.Controls.Window
    {
        private ImagePreviewWindow(ImageSource image, string title, string info)
        {
            Title = string.IsNullOrWhiteSpace(title)
                ? PropertyGridLib.Localization.LocalizationManager.GetString("ImagePreviewTitle")
                : title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.Manual;

            // 初始尺寸：工作区的 70%，并保证不超出屏幕
            var work = SystemParameters.WorkArea;
            Width = Math.Max(480, Math.Min(1200, work.Width * 0.7));
            Height = Math.Max(360, Math.Min(900, work.Height * 0.7));

            // 窗口底色 / 前景 / 标题栏配色全部由 hc:Window 的样式给出，
            // 这里刻意不设 Background —— 手动设反而会盖掉皮肤切换。

            var root = new Grid { Margin = new Thickness(12) };
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var imageBorder = new Border { BorderThickness = new Thickness(1), SnapsToDevicePixels = true };
            imageBorder.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
            imageBorder.SetResourceReference(Border.BackgroundProperty, "SecondaryRegionBrush");

            var imageHost = new Image
            {
                Source = image,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(6),
                // 位图按像素 1:1 渲染，放大时不做插值糊图
                SnapsToDevicePixels = true,
            };
            RenderOptions.SetBitmapScalingMode(imageHost, BitmapScalingMode.HighQuality);

            imageBorder.Child = imageHost;
            root.Children.Add(imageBorder);

            var footer = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(info) ? "—" : info,
                Margin = new Thickness(2, 8, 2, 0),
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            footer.SetResourceReference(TextBlock.ForegroundProperty, "SecondaryTextBrush");
            Grid.SetRow(footer, 1);
            root.Children.Add(footer);

            Content = root;

            // Esc 关闭（现场用键盘多，别逼人去点右上角）
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape) Close();
            };
        }

        /// <summary>以当前窗口为父打开大图预览（模态，Esc / 关闭按钮退出）。</summary>
        public static void ShowPreview(ImageSource image, string title, string info)
        {
            if (image == null) return;
            var win = new ImagePreviewWindow(image, title, info);
            var owner = Application.Current?.MainWindow;
            if (owner != null && owner.IsLoaded) win.Owner = owner;
            win.ShowDialog();
        }
    }
}
