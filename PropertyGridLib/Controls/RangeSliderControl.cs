using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 范围滑块控件（可脱离 PropertyGrid 独立使用，SDK）：
    /// 一根双滑块选择区间，按住拖动时在滑块上方显示「起点 ~ 终点」气泡
    /// （HandyControl 的 RangeSlider 模板不认 TipElement，这里用模板内 Border 自绘，
    /// 由鼠标按下/抬起切换显隐，数值随两端值变化刷新）。
    /// - <see cref="ValueStart"/> / <see cref="ValueEnd"/> 双向绑定两端值；
    /// - <see cref="Value"/> 以 <see cref="RangeValue"/> 整体双向绑定（属性网格行即绑定此属性）；
    /// - <see cref="Minimum"/> / <see cref="Maximum"/> / <see cref="Step"/> 配置边界与刻度；
    /// - <see cref="IsReadOnly"/> 时滑块禁用。
    /// 属性网格中声明 <see cref="RangeValue"/> 类型的属性会自动使用本控件（[RangeSlider] 可配边界）。
    /// </summary>
    public class RangeSliderControl : Control
    {
        static RangeSliderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSliderControl),
                new FrameworkPropertyMetadata(typeof(RangeSliderControl)));
        }

        // 防止 Value 与 ValueStart/ValueEnd 双向同步时互相打扰
        private bool _syncing;

        // ---------------- 范围值 ----------------

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(RangeValue), typeof(RangeSliderControl),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));

        /// <summary>整体区间值（双向）；null 时滑块停在默认位置</summary>
        public RangeValue Value
        {
            get => (RangeValue)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueStartProperty =
            DependencyProperty.Register(nameof(ValueStart), typeof(double), typeof(RangeSliderControl),
                new FrameworkPropertyMetadata(0d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueStartEndChanged));

        /// <summary>区间起点（双向）</summary>
        public double ValueStart
        {
            get => (double)GetValue(ValueStartProperty);
            set => SetValue(ValueStartProperty, value);
        }

        public static readonly DependencyProperty ValueEndProperty =
            DependencyProperty.Register(nameof(ValueEnd), typeof(double), typeof(RangeSliderControl),
                new FrameworkPropertyMetadata(100d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueStartEndChanged));

        /// <summary>区间终点（双向）</summary>
        public double ValueEnd
        {
            get => (double)GetValue(ValueEndProperty);
            set => SetValue(ValueEndProperty, value);
        }

        // ---------------- 配置 ----------------

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSliderControl),
                new PropertyMetadata(0d));

        /// <summary>可选取的最小值</summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSliderControl),
                new PropertyMetadata(100d));

        /// <summary>可选取的最大值</summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(double), typeof(RangeSliderControl),
                new PropertyMetadata(1d));

        /// <summary>每次增加的刻度（步长）</summary>
        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(RangeSliderControl),
                new PropertyMetadata(false));

        /// <summary>只读：滑块禁用（属性网格只读行使用）</summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        // ---------------- 拖动数值气泡 ----------------

        // 气泡画在控件模板内部（Popup 是独立窗口，属性行里会被裁剪/闪退，故不用 Popup）。
        // hc:RangeSlider 的交互不走 Thumb.DragStarted，这里按鼠标按下/抬起控制显隐，
        // 数值随 ValueStart/ValueEnd 变化刷新。
        private Border? _dragTip;
        private TextBlock? _dragTipText;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragTip = GetTemplateChild("PART_DragTip") as Border;
            _dragTipText = GetTemplateChild("PART_DragTipText") as TextBlock;

            AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown), true);
            AddHandler(PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(OnPreviewMouseUp), true);
            AddHandler(LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture), true);
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled || _dragTip == null) return;
            UpdateDragTipText();
            _dragTip.Visibility = Visibility.Visible;
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            HideDragTip();
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            HideDragTip();
        }

        private void HideDragTip()
        {
            if (_dragTip != null)
                _dragTip.Visibility = Visibility.Collapsed;
        }

        /// <summary>拖动气泡内容：起点 ~ 终点（滑块每移动一格都会经 ValueStart/ValueEnd 回调刷新）</summary>
        private void UpdateDragTipText()
        {
            if (_dragTipText != null)
                _dragTipText.Text = new RangeValue(ValueStart, ValueEnd).ToString();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RangeSliderControl)d;
            if (control._syncing) return;

            var value = (RangeValue)e.NewValue;
            if (value == null) return;

            // 用 SetCurrentValue 而不是 SetValue：后者会清掉外部（属性行）对本属性的 TwoWay 绑定
            control._syncing = true;
            control.SetCurrentValue(ValueStartProperty, value.Min);
            control.SetCurrentValue(ValueEndProperty, value.Max);
            control._syncing = false;
        }

        private static void OnValueStartEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RangeSliderControl)d;
            if (control._syncing) return;

            // 两端拖动后合成新的 RangeValue（不就地改旧实例，避免影响宿主共享对象）；
            // SetCurrentValue 保留 Value 上的 TwoWay 绑定，改动才能写回宿主属性
            control._syncing = true;
            control.SetCurrentValue(ValueProperty, new RangeValue(control.ValueStart, control.ValueEnd));
            control._syncing = false;

            // 拖动中的数值气泡实时跟随
            if (control._dragTip != null && control._dragTip.Visibility == Visibility.Visible)
                control.UpdateDragTipText();
        }
    }
}
