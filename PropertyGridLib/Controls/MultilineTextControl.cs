using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 多行文本（富文本查看）编辑器：由 [MultilineText] 特性触发的 string 属性编辑器。
    /// 行内保持「单行只读预览 + … 按钮」—— 不改变行高，属性面板虚拟化估算稳定；
    /// 点击 … 弹出多行富文本编辑窗口（自动换行 + 允许回车 + 独立滚动），确定后写回属性值。
    /// </summary>
    public class MultilineTextControl : Control
    {
        private Button _openButton;
        private Popup _popup;
        private TextBox _editBox;

        static MultilineTextControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultilineTextControl),
                new FrameworkPropertyMetadata(typeof(MultilineTextControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _openButton = GetTemplateChild("PART_OpenButton") as Button;
            if (_openButton != null)
            {
                _openButton.Click -= OpenButton_Click;
                _openButton.Click += OpenButton_Click;
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_popup != null && _popup.IsOpen)
            {
                ClosePopup();
                return;
            }

            if (DataContext is not PropertyItem pi) return;

            // 只读属性 → 只读浏览模式：正文只读、无「确定」，仅「关闭」；可编辑 → 正常编辑弹窗
            bool readOnly = pi.IsReadOnly;

            var root = new Border
            {
                Background = (Brush)FindResource("SecondaryRegionBrush"),
                BorderBrush = (Brush)FindResource("BorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10),
                MaxWidth = 560,
                Effect = new DropShadowEffect { BlurRadius = 12, ShadowDepth = 2, Opacity = 0.3, Color = Colors.Black }
            };

            // 布局：按钮区 Dock 在底部（永远保留一整行按钮空间，长正文把正文压短也不遮按钮），
            // 正文占据剩余空间
            var rootPanel = new DockPanel { LastChildFill = true, Margin = new Thickness(2) };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 8, 0, 0)
            };

            // 只读模式不放「确定」（没有可提交的内容），取消位显示「关闭」
            if (!readOnly)
            {
                var ok = new Button
                {
                    // 按钮文字走库内置词典（zh「确定」/ en「OK」），随语言切换
                    Content = LocalizationManager.GetString("OK"),
                    MinWidth = 72,
                    Margin = new Thickness(0, 0, 8, 0),
                    FontFamily = PropertyGrid.GlobalFontFamily,
                    FontSize = PropertyGrid.GlobalFontSize,
                    IsDefault = true
                };
                ok.Click += (_, _) => { Commit(pi); ClosePopup(); };
                buttons.Children.Add(ok);
            }

            var cancel = new Button
            {
                Content = LocalizationManager.GetString(readOnly ? "Close" : "Cancel"),
                MinWidth = 72,
                FontFamily = PropertyGrid.GlobalFontFamily,
                FontSize = PropertyGrid.GlobalFontSize,
                IsCancel = true
            };
            cancel.Click += (_, _) => ClosePopup();
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Bottom);
            rootPanel.Children.Add(buttons);

            _editBox = new TextBox
            {
                Text = pi.ValueString,
                AcceptsReturn = !readOnly,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MinHeight = 140,
                MaxHeight = 320,
                MinWidth = 380,
                FontFamily = PropertyGrid.GlobalFontFamily,
                FontSize = PropertyGrid.GlobalFontSize,
                VerticalContentAlignment = VerticalAlignment.Top,
                IsReadOnly = readOnly
            };
            rootPanel.Children.Add(_editBox);
            root.Child = rootPanel;

            // 屏幕空间检查：按钮行在屏幕下部时弹窗下半截会被屏幕边缘裁掉（实测"确定/取消"只剩半截）。
            // 剩余空间够 → 朝下弹；不够 → 朝上弹；都按剩余空间限高，按钮区永远可见。
            var targetScreen = _openButton.PointToScreen(new Point(0, _openButton.ActualHeight));
            var workArea = SystemParameters.WorkArea;
            // PointToScreen 返回物理像素，按各自轴向转回 WPF 逻辑单位（多显示器 DPI 可能不同）
            var dpi = VisualTreeHelper.GetDpi(_openButton);
            var targetBottomLogical = targetScreen.Y / dpi.DpiScaleY;

            var spaceBelow = workArea.Bottom - targetBottomLogical - 12;
            const double buttonBar = 52;                // 按钮行 + 边框内边距
            const double minBody = 120;                 // 正文最低可用高度

            double maxBody;
            PlacementMode placement;
            if (spaceBelow >= minBody + buttonBar)
            {
                // 下方够：朝下弹，正文高度 = 剩余空间 - 按钮区
                placement = PlacementMode.Bottom;
                maxBody = Math.Min(320, spaceBelow - buttonBar);
            }
            else
            {
                // 下方不够：朝上弹，正文高度 = 目标上方空间 - 按钮区
                placement = PlacementMode.Top;
                var spaceAbove = targetBottomLogical - _openButton.ActualHeight - workArea.Top - 12;
                maxBody = Math.Max(minBody, Math.Min(320, spaceAbove - buttonBar));
            }
            _editBox.MaxHeight = maxBody;
            root.MaxHeight = maxBody + buttonBar;

            _popup = new Popup
            {
                Placement = placement,
                PlacementTarget = _openButton,
                StaysOpen = true,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Slide,
                Focusable = true,
                Child = root
            };
            _popup.Closed += (_, _) => _editBox = null;
            _popup.IsOpen = true;
            _editBox.Focus();
            if (!readOnly)
                _editBox.SelectAll();
        }

        private void Commit(PropertyItem pi)
        {
            if (_editBox == null) return;
            // 走 ValueString 通道：内部完成字符串→目标类型转换与防抖比较
            pi.ValueString = _editBox.Text;
        }

        private void ClosePopup()
        {
            if (_popup != null) _popup.IsOpen = false;
        }
    }
}
