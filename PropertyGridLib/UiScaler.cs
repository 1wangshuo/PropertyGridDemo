using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace PropertyGridLib
{
    /// <summary>
    /// 窗口分辨率/DPI 自适应助手（PropertyGridLib 内部版，与宿主 TeamAAS.Dialogs.UiScaler 同源）。
    ///
    /// 设计：全应用使用"同一个全局缩放因子"——
    ///   主窗口（铺满屏幕）：按设计基准计算因子；
    ///   弹窗/子窗体：跟随全局因子，只放大不缩小（避免小窗被缩到看不清），
    ///   窗口尺寸按设计尺寸 x 因子放大，内容同步缩放（不溢出、不裁切）。
    ///
    /// 本类在独立 Demo 中可自管因子（AttachMain）；嵌入宿主时由宿主注入外部因子：
    ///   // 宿主启动处一次性接线（App.xaml.cs 等）：
    ///   PropertyGridLib.UiScaler.ExternalScaleProvider = () => TeamAAS.Dialogs.UiScaler.GlobalScale;
    ///   TeamAAS.Dialogs.UiScaler.ScaleChanged += () =>
    ///       PropertyGridLib.UiScaler.SetExternalScale(TeamAAS.Dialogs.UiScaler.GlobalScale);
    ///
    /// 用法：库内弹窗构造函数中调用 UiScaler.Attach(this);
    /// </summary>
    public static class UiScaler
    {
        private static double _globalScale = 1.0;

        /// <summary>当前全局缩放因子（弹窗读取用；设置了 ExternalScaleProvider 时以其返回值为准）</summary>
        public static double GlobalScale => ExternalScaleProvider?.Invoke() ?? _globalScale;

        /// <summary>获取系统 DPI 缩放因子（1.0 = 100%，1.5 = 150%，2.0 = 200%）</summary>
        private static double GetSystemDpiScale()
        {
            try
            {
                var source = PresentationSource.FromVisual(Application.Current.MainWindow);
                if (source?.CompositionTarget != null)
                {
                    return source.CompositionTarget.TransformToDevice.M11;
                }
            }
            catch { }
            return 1.0;
        }

        /// <summary>
        /// 外部缩放因子提供者：宿主程序注入（如 () => 宿主主窗口算出的全局因子），
        /// 库内弹窗即可与宿主保持同一缩放，保证跨分辨率观感一致。
        /// </summary>
        public static Func<double> ExternalScaleProvider;

        /// <summary>全局因子变化通知（本类内部挂接的弹窗订阅后自动跟随）</summary>
        public static event Action ScaleChanged;

        /// <summary>
        /// 宿主全局因子变化时调用：刷新所有已挂接的库内弹窗（幂等、线程安全到 UI 调用方）。
        /// </summary>
        public static void SetExternalScale(double scale)
        {
            _globalScale = Math.Max(0.1, Math.Min(5.0, scale));
            _hasExplicitGlobal = true;
            ScaleChanged?.Invoke();
        }

        private static bool _hasExplicitGlobal;

        /// <summary>
        /// 解析当前应使用的缩放因子（弹窗占屏幕比例恒定，观感不随分辨率变化）：
        /// 1) 宿主通过 ExternalScaleProvider 注入 —— 绝对服从（TeamAAS 主窗体口径）；
        /// 2) 本类 AttachMain/SetExternalScale 已显式设置 —— 使用之；
        /// 3) 均未配置 —— 按"设计基准 1600×900 ÷ 当前屏幕工作区(DPI 逻辑像素)"推导，
        ///    使弹窗在 1600×900 与 4K 等不同分辨率下占据屏幕的比例一致（只放大不缩小，
        ///    上限 2.0 防止失控）。这正是"TeamAAS 主窗体铺满屏幕等比缩放"在子窗体的对应实现。
        /// 
        /// 注意：此值会在 ApplyTransform 中统一除以系统 DPI 因子，
        /// 防止 WPF DPI 渲染 + LayoutTransform ScaleTransform 双重缩放导致高 DPI 屏幕上对话框膨胀。
        /// </summary>
        private static double ResolveScale()
        {
            var external = ExternalScaleProvider?.Invoke();
            if (external.HasValue && external.Value > 0)
                return Math.Max(1.0, external.Value);

            if (_hasExplicitGlobal)
                return Math.Max(1.0, _globalScale);

            // 自动推导：屏幕工作区越小因子越接近 1.0，越大则整体放大，保证占比恒定。
            var wa = SystemParameters.WorkArea;
            double f = Math.Min(wa.Width / 1600.0, wa.Height / 900.0);

            return Math.Max(1.0, Math.Min(2.0, f));
        }

        private static readonly Dictionary<Window, Size> _designSizes = new Dictionary<Window, Size>();

        /// <summary>
        /// 主窗口：计算全局缩放因子并应用（仅独立宿主/Demo 使用；TeamAAS 有自己的 UiScaler）。
        /// 设计基准默认 1600x900，钳制 minScale~maxScale。
        /// </summary>
        public static void AttachMain(Window window, double refWidth = 1600, double refHeight = 900,
                                      double minScale = 0.6, double maxScale = 2.0)
        {
            if (window == null) return;
            var root = window.Content as FrameworkElement;
            if (root == null) return;
            if (root.Tag is UiScalerToken) return;
            root.Tag = new UiScalerToken();

            void Update()
            {
                double w = window.ActualWidth > 0 ? window.ActualWidth : refWidth;
                double h = window.ActualHeight > 0 ? window.ActualHeight : refHeight;

                double f = Math.Min(w / refWidth, h / refHeight);
                _globalScale = Math.Max(minScale, Math.Min(maxScale, f));
                _hasExplicitGlobal = true;

                ApplyTransform(window, GlobalScale, resizeWindow: false);
                ScaleChanged?.Invoke();
            }

            window.SizeChanged += (s, e) => Update();
            window.DpiChanged += (s, e) => Update();
            window.Loaded += (s, e) => Update();
            SystemEvents.DisplaySettingsChanged += (s, e) => window.Dispatcher.Invoke(Update);
        }

        /// <summary>
        /// 弹窗/子窗体：跟随全局缩放因子（只放大不缩小，窗口尺寸与内容同步缩放）。
        /// 因子来源见 <see cref="ResolveScale"/>（宿主注入 > 显式计算 > 屏幕自动推导），
        /// 因此零接线也能在各分辨率下保持一致观感。幂等：重复调用安全。
        /// </summary>
        public static void Attach(Window window)
        {
            if (window == null) return;
            var root = window.Content as FrameworkElement;
            if (root == null) return;
            if (root.Tag is UiScalerToken) return;
            root.Tag = new UiScalerToken();

            Action apply = () => ApplyTransform(window, ResolveScale(), resizeWindow: true);
            apply();

            // 全局因子变化时跟随；DPI 变化（拖到另一块屏幕/系统缩放调整）时重算；
            // 关闭时退订，避免事件泄漏（注意：退订必须使用同一委托实例）
            ScaleChanged += apply;
            System.Windows.DpiChangedEventHandler dpiHandler = (s, e) => apply();
            window.DpiChanged += dpiHandler;
            window.Closed += (s, e) =>
            {
                ScaleChanged -= apply;
                window.DpiChanged -= dpiHandler;
            };
        }

        private static void ApplyTransform(Window w, double f, bool resizeWindow)
        {
            var root = w.Content as FrameworkElement;
            if (root == null) return;

            // 弹窗只放大不缩小（f<1 时保持 1.0，避免小窗被缩到看不清）
            double effective = resizeWindow ? Math.Max(1.0, f) : f;

            // 除以系统 DPI 缩放因子：WPF 已在渲染层按 DPI 缩放内容，
            // LayoutTransform 的 ScaleTransform 会在此之上再次缩放，
            // 若不扣除 DPI 因子，高 DPI 屏幕上对话框物理尺寸会膨胀。
            double dpiScale = GetSystemDpiScale();
            effective = Math.Max(1.0, effective / Math.Max(1.0, dpiScale));

            if (Math.Abs(effective - 1.0) < 0.001)
            {
                root.LayoutTransform = null;
                return;
            }

            if (resizeWindow)
            {
                // 首次记录设计尺寸（XAML 声明值；NaN=自动/SizeToContent 交由内容自适应）
                if (!_designSizes.TryGetValue(w, out var design))
                {
                    design = new Size(w.Width, w.Height);
                    _designSizes[w] = design;
                }
                if (!double.IsNaN(design.Width) && design.Width > 0) w.Width = design.Width * effective;
                if (!double.IsNaN(design.Height) && design.Height > 0) w.Height = design.Height * effective;
            }

            root.LayoutTransform = new ScaleTransform(effective, effective);
        }

        private sealed class UiScalerToken { }
    }
}
