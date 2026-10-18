using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 颜色编辑器控件：颜色预览 + 十六进制文本 + ...按钮(Popup 弹出 hc:ColorPicker)。
    /// 对齐 FilePath/Dictionary 等编辑器的布局风格。
    /// </summary>
    public class ColorEditControl : Control
    {
        private Button _colorButton;
        private Popup _popup;
        private HandyControl.Controls.ColorPicker _colorPicker;
        private Border _popupBorder;

        static ColorEditControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorEditControl),
                new FrameworkPropertyMetadata(typeof(ColorEditControl)));
        }

        /// <summary>
        /// 色块预览专用画刷：**本元素独占、不冻结、只就地改 Color**。
        /// 关键点：色块 Background 的绑定值从头到尾是同一个实例，值变化时不发生「换画刷」，
        /// WPF 也就不会走「把旧画刷从组合通道上释放」那条路径 —— 该路径在虚拟化回收/连续重置场景下
        /// 会抛 COMException 0x8000FFFF（E_UNEXPECTED），实测「重置所有属性」第二次必崩
        /// （SolidColorBrush.ReleaseOnChannelCore → Visual.DisconnectAttachedResource → RenderClose → Arrange）。
        /// 之前走 ColorToBrushConverter 每次求值都产生新实例，正是这个释放循环的来源。
        /// </summary>
        private readonly SolidColorBrush _swatchBrush = new SolidColorBrush(Colors.Transparent);

        /// <summary>模板里色块 Border 的 Background 绑定到此（同一实例，只改 Color）</summary>
        public Brush SwatchBrush => _swatchBrush;

        private System.ComponentModel.INotifyPropertyChanged _watchedItem;

        public ColorEditControl()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_watchedItem != null) _watchedItem.PropertyChanged -= OnItemPropertyChanged;
            _watchedItem = e.NewValue as System.ComponentModel.INotifyPropertyChanged;
            if (_watchedItem != null) _watchedItem.PropertyChanged += OnItemPropertyChanged;
            UpdateSwatch();
        }

        private void OnItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyItem.Value) || e.PropertyName == nameof(PropertyItem.ValueString))
                UpdateSwatch();
        }

        private void UpdateSwatch()
        {
            var color = DataContext is PropertyItem pi ? ValueToColor(pi.Value) : (Color?)null;
            _swatchBrush.Color = color ?? Colors.Transparent;
        }

        private static Color? ValueToColor(object value)
        {
            if (value is Color c) return c;
            if (value is SolidColorBrush scb) return scb.Color;
            return null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _colorButton = GetTemplateChild("PART_ColorButton") as Button;
            if (_colorButton != null)
            {
                _colorButton.Click -= ColorButton_Click;
                _colorButton.Click += ColorButton_Click;
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_popup != null && _popup.IsOpen)
            {
                ClosePopup();
                return;
            }

            // 创建 Popup + ColorPicker
            // StaysOpen=true + 手动处理外部点击关闭 — 避免 HC ColorPicker 内部 Slider 拖动时
            // 误触发"点击外部"导致 Popup 提前关闭的问题
            _popup = new Popup
            {
                Placement = PlacementMode.Bottom,
                PlacementTarget = _colorButton,
                StaysOpen = true,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Slide,
                Focusable = true
            };

            _popupBorder = new Border
            {
                Background = (Brush)FindResource("RegionBrush"),
                BorderBrush = (Brush)FindResource("BorderBrush"),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.3, Color = Colors.Black }
            };

            _colorPicker = new HandyControl.Controls.ColorPicker
            {
                Margin = new Thickness(4),
                FontFamily = PropertyGrid.GlobalFontFamily,
                FontSize = PropertyGrid.GlobalFontSize
            };

            // 监听 HC ColorPicker 的 确定/取消 事件
            _colorPicker.Confirmed -= ColorPicker_Confirmed;
            _colorPicker.Confirmed += ColorPicker_Confirmed;
            _colorPicker.Canceled -= ColorPicker_Canceled;
            _colorPicker.Canceled += ColorPicker_Canceled;

            // 同步初始颜色
            SyncPickerFromPropertyItem();

            _popupBorder.Child = _colorPicker;
            _popup.Child = _popupBorder;
            _popup.IsOpen = true;
            _popup.Closed += Popup_Closed;

            // 监听鼠标点击，点外部则关闭
            PresentationSource.AddSourceChangedHandler(this, SourceChanged);
            if (PresentationSource.FromVisual(this) is PresentationSource source)
            {
                ((UIElement)source.RootVisual).AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Window_PreviewMouseDown), true);
            }
        }

        private void SourceChanged(object sender, SourceChangedEventArgs e)
        {
            if (e.OldSource is PresentationSource oldSource && oldSource.RootVisual is UIElement oldRoot)
            {
                oldRoot.RemoveHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Window_PreviewMouseDown));
            }
            if (e.NewSource is PresentationSource newSource && newSource.RootVisual is UIElement newRoot)
            {
                newRoot.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Window_PreviewMouseDown), true);
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_popup == null || !_popup.IsOpen) return;

            if (_popupBorder != null && IsInVisualTree(e.OriginalSource as DependencyObject, _popupBorder))
                return;

            if (_colorButton != null && IsInVisualTree(e.OriginalSource as DependencyObject, _colorButton))
                return;

            ClosePopup();
        }

        private static bool IsInVisualTree(DependencyObject element, DependencyObject root)
        {
            if (element == null || root == null) return false;
            var current = element;
            while (current != null)
            {
                if (current == root) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        private void ClosePopup()
        {
            if (_popup != null) _popup.IsOpen = false;
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            if (PresentationSource.FromVisual(this) is PresentationSource source && source.RootVisual is UIElement root)
            {
                root.RemoveHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(Window_PreviewMouseDown));
            }
            PresentationSource.RemoveSourceChangedHandler(this, SourceChanged);

            _colorPicker = null;
            _popupBorder = null;
            _popup = null;
        }

        /// <summary>用户点击 HC ColorPicker 的"确定"按钮 — 此时才应用颜色值</summary>
        private void ColorPicker_Confirmed(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            if (DataContext is not PropertyItem pi) { ClosePopup(); return; }

            var selectedColor = e.Info;

            var targetType = pi.EffectivePropertyType;
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying == typeof(Color))
            {
                pi.Value = selectedColor;
            }
            else if (underlying == typeof(SolidColorBrush))
            {
                // 走共享缓存：与色块 Background 用的是同一个冻结实例，避免每次新建/释放
                pi.Value = Converters.ColorToBrushConverter.GetFrozenBrush(selectedColor);
            }
            else
            {
                pi.Value = selectedColor;
            }

            ClosePopup();
        }

        /// <summary>用户点击 HC ColorPicker 的"取消"按钮 — 不应用任何值,直接关闭</summary>
        private void ColorPicker_Canceled(object sender, EventArgs e)
        {
            ClosePopup();
        }

        private void SyncPickerFromPropertyItem()
        {
            if (_colorPicker == null || DataContext is not PropertyItem pi || pi.Value == null) return;

            var brush = ValueToBrush(pi.Value);
            if (brush != null) _colorPicker.SelectedBrush = brush;
        }

        private static SolidColorBrush ValueToBrush(object value)
        {
            if (value == null) return null;
            if (value is SolidColorBrush scb) return scb;
            // Color → 共享的冻结画刷（每次 new 再 Freeze 会让旧实例在布局里被释放，见 ColorToBrushConverter 注释）
            if (value is Color c) return Converters.ColorToBrushConverter.GetFrozenBrush(c);
            return null;
        }
    }
}
