using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 单值滑块控件（可脱离 PropertyGrid 独立使用）：
    /// 标准滑块 + 按住拖动时上方的数值气泡，交互与 <see cref="RangeSliderControl"/> 一致
    /// （按下显示、抬起隐藏，值变化时气泡实时刷新）。
    /// - <see cref="Value"/> 双向绑定当前值；
    /// - <see cref="Minimum"/> / <see cref="Maximum"/> / <see cref="Step"/> 配置边界与刻度；
    /// - <see cref="IsReadOnly"/> 时滑块禁用；
    /// - <see cref="DisplayText"/> 可绑定宿主格式化后的文本（如属性行的 ValueString），为空时显示 Value。
    /// 属性网格中带 [NumberSlider] 的属性行内部即此控件。
    /// </summary>
    public class NumberSliderControl : Control
    {
        static NumberSliderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberSliderControl),
                new FrameworkPropertyMetadata(typeof(NumberSliderControl)));
        }

        // ---------------- 值 ----------------

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumberSliderControl),
                new FrameworkPropertyMetadata(0d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));

        /// <summary>当前值（双向）</summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        // ---------------- 配置 ----------------

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumberSliderControl),
                new PropertyMetadata(0d));

        /// <summary>可选取的最小值</summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumberSliderControl),
                new PropertyMetadata(100d));

        /// <summary>可选取的最大值</summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(double), typeof(NumberSliderControl),
                new PropertyMetadata(1d));

        /// <summary>每次增加的刻度（步长）</summary>
        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(NumberSliderControl),
                new PropertyMetadata(false));

        /// <summary>只读：滑块禁用（属性网格只读行使用）</summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        // ---------------- 拖动数值气泡 ----------------

        /// <summary>气泡显示文本（一般绑属性行的 ValueString，保留宿主格式化）；为空时用 Value 的数值。</summary>
        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(NumberSliderControl),
                new PropertyMetadata(null, OnDisplayTextChanged));

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        private static readonly DependencyPropertyKey BubbleTextPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(BubbleText), typeof(string), typeof(NumberSliderControl),
                new PropertyMetadata("0"));

        public static readonly DependencyProperty BubbleTextProperty = BubbleTextPropertyKey.DependencyProperty;

        /// <summary>气泡实际显示的文本</summary>
        public string BubbleText => (string)GetValue(BubbleTextProperty);

        private Border _dragTip;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragTip = GetTemplateChild("PART_DragTip") as Border;

            AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown), true);
            AddHandler(PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(OnPreviewMouseUp), true);
            AddHandler(LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture), true);
            UpdateBubbleText();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled || _dragTip == null) return;
            UpdateBubbleText();
            _dragTip.Visibility = Visibility.Visible;
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e) => HideDragTip();

        private void OnLostMouseCapture(object sender, MouseEventArgs e) => HideDragTip();

        private void HideDragTip()
        {
            if (_dragTip != null)
                _dragTip.Visibility = Visibility.Collapsed;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((NumberSliderControl)d).UpdateBubbleText();

        private static void OnDisplayTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((NumberSliderControl)d).UpdateBubbleText();

        private void UpdateBubbleText()
        {
            var text = DisplayText;
            if (string.IsNullOrEmpty(text))
                text = Value.ToString("0.###", CultureInfo.CurrentCulture);
            SetValue(BubbleTextPropertyKey, text);
        }
    }
}
