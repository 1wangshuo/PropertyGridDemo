using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PropertyGridLib.Behaviors
{
    /// <summary>
    /// 触屏纵向滑动：打开后，手指按在列表行或行内编辑器上都能像列表一样拖动滚动。
    /// 点击和横向拖动不拦截，仍交给行内控件（开关切换、滑块调值、文本框选字）。
    /// 用法：&lt;ScrollViewer behaviors:TouchScroll.IsEnabled="True" /&gt;
    /// </summary>
    public static class TouchScroll
    {
        /// <summary>手指至少移动这么多像素才认定是滑动，避免把点击吞掉</summary>
        private const double StartDistance = 6;

        /// <summary>松手速度低于这个值（像素/毫秒）就不做惯性滚动</summary>
        private const double MinInertiaSpeed = 0.4;

        /// <summary>惯性滚动每 16 毫秒保留的速度比例</summary>
        private const double InertiaDecay = 0.92;

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TouchScroll),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

        /// <summary>一个滚动容器同一时刻只跟一根手指，状态按容器分开保存</summary>
        private sealed class DragState
        {
            public TouchDevice? Device;
            public Point Start;
            public double StartOffset;
            public double LastY;
            public int LastTime;
            public double Speed;      // 手指速度：像素/毫秒，向下滑为正
            public bool IsPanning;
            public DispatcherTimer? Inertia;
        }

        private static readonly ConditionalWeakTable<ScrollViewer, DragState> States =
            new ConditionalWeakTable<ScrollViewer, DragState>();

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer viewer)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                viewer.PreviewTouchDown += OnTouchDown;
                viewer.PreviewTouchMove += OnTouchMove;
                viewer.PreviewTouchUp += OnTouchUp;
                viewer.LostTouchCapture += OnLostTouchCapture;
                viewer.Unloaded += OnUnloaded;
            }
            else
            {
                viewer.PreviewTouchDown -= OnTouchDown;
                viewer.PreviewTouchMove -= OnTouchMove;
                viewer.PreviewTouchUp -= OnTouchUp;
                viewer.LostTouchCapture -= OnLostTouchCapture;
                viewer.Unloaded -= OnUnloaded;
                StopInertia(viewer);
            }
        }

        private static void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopInertia((ScrollViewer)sender);
        }

        /// <summary>触摸被别的控件抢走时结束本次拖动，避免下次按下去还被当成“已有手指在拖”</summary>
        private static void OnLostTouchCapture(object? sender, TouchEventArgs e)
        {
            var state = States.GetValue((ScrollViewer)sender!, _ => new DragState());
            state.IsPanning = false;
        }

        private static void OnTouchDown(object? sender, TouchEventArgs e)
        {
            var viewer = (ScrollViewer)sender!;
            var state = States.GetValue(viewer, _ => new DragState());

            // 已经有一根手指在拖动，第二根手指不打断
            if (state.IsPanning)
            {
                return;
            }

            StopInertia(viewer);

            state.Device = e.TouchDevice;
            state.Start = e.GetTouchPoint(viewer).Position;
            state.StartOffset = viewer.VerticalOffset;
            state.LastY = state.Start.Y;
            state.LastTime = Environment.TickCount;
            state.Speed = 0;
        }

        private static void OnTouchMove(object? sender, TouchEventArgs e)
        {
            var viewer = (ScrollViewer)sender!;
            var state = States.GetValue(viewer, _ => new DragState());
            if (state.Device == null || state.Device.Id != e.TouchDevice.Id)
            {
                return;
            }

            var point = e.GetTouchPoint(viewer).Position;

            if (!state.IsPanning)
            {
                var dx = point.X - state.Start.X;
                var dy = point.Y - state.Start.Y;

                // 还没滑够距离或明显是横向手势：不抢，留给行内控件（滑块、文本框选字）
                if (Math.Abs(dy) < StartDistance || Math.Abs(dy) < Math.Abs(dx))
                {
                    return;
                }

                // 列表本来就不能滚，抢走手势没有意义
                if (viewer.ScrollableHeight <= 0)
                {
                    return;
                }

                // 抓不到也继续：后续 TouchMove 仍会经过本容器的预览事件，行内控件已被 Handled 挡住
                viewer.CaptureTouch(e.TouchDevice);

                // 松开行内控件按下时抓的鼠标，否则滑完手指抬起还会补发一次点击
                Mouse.Capture(null);
                state.IsPanning = true;
            }

            e.Handled = true;

            var now = Environment.TickCount;
            var elapsed = now - state.LastTime;
            if (elapsed > 0 && elapsed < 100)
            {
                state.Speed = (point.Y - state.LastY) / elapsed;
            }
            state.LastY = point.Y;
            state.LastTime = now;

            viewer.ScrollToVerticalOffset(state.StartOffset - (point.Y - state.Start.Y) / GetPixelsPerItem(viewer));
        }

        private static void OnTouchUp(object? sender, TouchEventArgs e)
        {
            var viewer = (ScrollViewer)sender!;
            var state = States.GetValue(viewer, _ => new DragState());
            if (state.Device == null || state.Device.Id != e.TouchDevice.Id)
            {
                return;
            }

            var wasPanning = state.IsPanning;
            state.Device = null;
            state.IsPanning = false;

            if (!wasPanning)
            {
                return;
            }

            e.Handled = true;

            // 手指停住再抬起不算甩动
            if (Environment.TickCount - state.LastTime > 100)
            {
                state.Speed = 0;
            }

            if (Math.Abs(state.Speed) >= MinInertiaSpeed)
            {
                StartInertia(viewer, state);
            }
        }

        /// <summary>松手后按手指速度继续滚一段，逐帧减速直到停下或到头</summary>
        private static void StartInertia(ScrollViewer viewer, DragState state)
        {
            StopInertia(viewer);

            var pixelsPerItem = GetPixelsPerItem(viewer);
            var speed = state.Speed;
            var lastTime = Environment.TickCount;

            var timer = new DispatcherTimer(DispatcherPriority.Input)
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            timer.Tick += (_, __) =>
            {
                var now = Environment.TickCount;
                var elapsed = Math.Min(now - lastTime, 50);
                lastTime = now;
                if (elapsed <= 0)
                {
                    return;
                }

                var offset = viewer.VerticalOffset - speed * elapsed / pixelsPerItem;
                viewer.ScrollToVerticalOffset(offset);
                speed *= Math.Pow(InertiaDecay, elapsed / 16.0);

                if (Math.Abs(speed) < MinInertiaSpeed || offset <= 0 || offset >= viewer.ScrollableHeight)
                {
                    timer.Stop();
                    state.Inertia = null;
                }
            };

            state.Inertia = timer;
            timer.Start();
        }

        private static void StopInertia(ScrollViewer viewer)
        {
            var state = States.GetValue(viewer, _ => new DragState());
            state.Inertia?.Stop();
            state.Inertia = null;
        }

        /// <summary>
        /// 手指移动的像素换算成滚动条的单位：
        /// 项滚动（CanContentScroll=True）时偏移单位是“行”，要按视口估算每行像素高；
        /// 像素滚动时 1:1 跟手。
        /// </summary>
        private static double GetPixelsPerItem(ScrollViewer viewer)
        {
            if (!viewer.CanContentScroll)
            {
                return 1;
            }

            var visibleItems = viewer.ViewportHeight;
            if (visibleItems <= 0 || viewer.ActualHeight <= 0)
            {
                return 36;
            }

            return Math.Max(1, viewer.ActualHeight / visibleItems);
        }
    }
}
