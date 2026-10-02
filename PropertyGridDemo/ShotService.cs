using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HandyControl.Data;
using PropertyGridLib;
using PropertyGridLib.Controls;
using PropertyGridDemo.Models;

namespace PropertyGridDemo
{
    /// <summary>
    /// 离屏截图工具：以命令行 --shots &lt;目录&gt; 触发，用 RenderTargetBitmap 把真实控件渲染成 PNG，
    /// 生成文档配图（零水印、可控尺寸、明暗主题）。仅 Demo 侧使用，不参与发布包。
    /// </summary>
    public static class ShotService
    {
        private static string _dir;

        public static void Run(string outDir)
        {
            _dir = outDir;
            Directory.CreateDirectory(outDir);
            // 截图期间逐个 Show/Close 窗口，避免默认 OnLastWindowClose 触发应用关闭
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var dispatcher = Application.Current.Dispatcher;

            // ===== 浅色 =====
            ThemeHelper.Apply(SkinType.Default);

            TryShot("propertygrid-main-light", () =>
            {
                var w = NewMainWindow();
                {
                    Show(w, 1240, 820);
                    Save(w, "propertygrid-main-light");
                    w.Close();
                }
            });

            TryShot("pro-overview-light", () =>
            {
                var w = NewProWindow();
                {
                    Show(w, 1200, 800);
                    Save(w, "pro-overview-light");
                    w.Close();
                }
            });

            TryShot("pro-card-layout-detail", () =>
            {
                var w = NewProWindow();
                {
                    Show(w, 1200, 800);
                    var g = Find(w, "proGrid");
                    if (g != null) SaveElementCrop(g, 0, 192, 0, 400, "pro-card-layout-detail");
                    w.Close();
                }
            });

            TryShot("toggle-switch-area-light", () =>
            {
                var w = NewProWindow();
                {
                    Show(w, 1200, 800);
                    var g = Find(w, "proGrid");
                    if (g != null)
                    {
                        g.SearchText = "开关";
                        w.UpdateLayout();
                        dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                        SaveElementCrop(g, 0, 192, 0, 400, "toggle-switch-area-light");
                    }
                    w.Close();
                }
            });

            TryShot("pro-search-multi-light", () =>
            {
                var w = NewProWindow();
                {
                    Show(w, 1200, 800);
                    var g = Find(w, "proGrid");
                    if (g != null)
                    {
                        g.SearchText = "摄像";
                        w.UpdateLayout();
                        dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                        SaveElementCrop(g, 0, 192, 0, 400, "pro-search-multi-light");
                    }
                    w.Close();
                }
            });

            TryShot("pro-nested-4level-light", () =>
            {
                var w = NewProWindow();
                {
                    Show(w, 1200, 800);
                    var g = Find(w, "proGrid");
                    if (g != null)
                    {
                        g.HeaderDescription = "4 层嵌套子属性示例：展开后逐级缩进";
                        g.SourceText = "来源扩展：NestZone";
                        g.SelectedObject = new NestZone();
                        g.SetSubPropertiesExpanded(true);
                        w.UpdateLayout();
                        dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                        SaveElementCrop(g, 0, 192, 0, 540, "pro-nested-4level-light");
                    }
                    w.Close();
                }
            });

            TryShot("reset-button-base-light", () =>
            {
                var w = NewMainWindow();
                {
                    Show(w, 1240, 820);
                    var grid = LogicalTreeHelper.FindLogicalNode(w, "propertyGrid") as PropertyGrid;
                    if (grid != null)
                    {
                        ModifyOne(grid, "启用状态", false);
                        ModifyOne(grid, "摄像头名称", "后堂");
                        w.UpdateLayout();
                        dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                        SaveElementCrop(grid, 0, 140, 0, 460, "reset-button-base-light");
                    }
                    w.Close();
                }
            });

            // ===== 深色 =====
            ThemeHelper.Apply(SkinType.Dark);

            TryShot("propertygrid-main-dark", () =>
            {
                var w = NewMainWindow();
                { Show(w, 1240, 820); Save(w, "propertygrid-main-dark"); w.Close(); }
            });

            TryShot("pro-overview-dark", () =>
            {
                var w = NewProWindow();
                { Show(w, 1200, 800); Save(w, "pro-overview-dark"); w.Close(); }
            });

            ThemeHelper.Apply(SkinType.Default);
            Log("SHOT_DONE");
        }

        // ---------- 窗体构造 ----------
        private static MainWindow NewMainWindow()
        {
            var w = new MainWindow();
            w.WindowState = WindowState.Normal;
            return w;
        }

        private static PropertyGridProDemoWindow NewProWindow()
        {
            return new PropertyGridProDemoWindow();
        }

        private static PropertyGridPro Find(Window w, string name)
        {
            return LogicalTreeHelper.FindLogicalNode(w, name) as PropertyGridPro;
        }

        // 把指定显示名的属性改为非初始值，使行内“R”重置按钮常显
        private static void ModifyOne(PropertyGrid grid, string displayName, object newVal)
        {
            if (!(grid.GroupedCategories is System.Collections.Generic.IEnumerable<PropertyCategory> cats)) return;
            foreach (var c in cats)
                foreach (var it in c.Items)
                    if (it is PropertyItem pi && pi.DisplayName == displayName)
                    {
                        try { pi.Value = newVal; } catch { }
                        return;
                    }
        }

        // ---------- 显示并等待渲染 ----------
        private static void Show(Window w, double width, double height)
        {
            w.WindowStartupLocation = WindowStartupLocation.Manual;
            w.Left = -10000; w.Top = -10000;
            w.Width = width; w.Height = height;
            w.ResizeMode = ResizeMode.NoResize;
            w.ShowInTaskbar = false;
            w.ShowActivated = false;
            w.AllowsTransparency = false;
            w.Show();
            var d = w.Dispatcher;
            d.Invoke(() => w.UpdateLayout(), DispatcherPriority.Loaded);
            d.Invoke(() => { }, DispatcherPriority.Render);
            d.Invoke(() => { }, DispatcherPriority.ContextIdle);
        }

        // ---------- 渲染保存 ----------
        private static void Save(Visual v, string name)
        {
            var fe = v as FrameworkElement;
            int w = (int)Math.Ceiling(fe.ActualWidth > 0 ? fe.ActualWidth : fe.Width);
            int h = (int)Math.Ceiling(fe.ActualHeight > 0 ? fe.ActualHeight : fe.Height);
            if (w <= 0 || h <= 0) { Log(name + ": size 0"); return; }
            RenderTo(v, w, h, name);
        }

        // 按控件自然尺寸渲染，再裁剪 (x,y,w,h) 区域（w/h 传 0 表示到边界）
        private static void SaveElementCrop(FrameworkElement el, int x, int y, int w, int h, string name)
        {
            el.UpdateLayout();
            Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            int aw = (int)Math.Ceiling(el.ActualWidth);
            int ah = (int)Math.Ceiling(el.ActualHeight);
            if (aw <= 0 || ah <= 0) { Log(name + ": el size 0"); return; }
            var full = new RenderTargetBitmap(aw, ah, 96, 96, PixelFormats.Pbgra32);
            full.Render(el);
            int cw = w > 0 ? Math.Min(w, aw - x) : aw - x;
            int ch = h > 0 ? Math.Min(h, ah - y) : ah - y;
            BitmapSource outImg = new CroppedBitmap(full, new Int32Rect(x, y, cw, ch));
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(outImg));
            var path = Path.Combine(_dir, name + ".png");
            using (var fs = File.Create(path)) enc.Save(fs);
            Log(name + " -> " + cw + "x" + ch);
        }

        private static void RenderTo(Visual v, int w, int h, string name)
        {
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(v);
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));
            var path = Path.Combine(_dir, name + ".png");
            using (var fs = File.Create(path)) enc.Save(fs);
            Log(name + " -> " + w + "x" + h);
        }

        private static void TryShot(string name, Action a)
        {
            try { a(); }
            catch (Exception ex) { Log("FAIL " + name + ": " + ex.Message); }
        }

        private static void Log(string s)
        {
            try { Console.WriteLine("[shots] " + s); } catch { }
            try { File.AppendAllText(Path.Combine(_dir, "_shots.log"), s + Environment.NewLine); } catch { }
        }
    }

    // ===== 文档配图专用：4 层嵌套子属性模型 =====
    public class NestZone
    {
        [Category("检测区域")][DisplayName("区域名称")] public string Name { get; set; } = "主区域";
        [Category("检测区域")][DisplayName("灵敏度")][DefaultValue(5)] public int Sensitivity { get; set; } = 7;
        [Category("检测区域")][DisplayName("子区域")] public NestSub Sub { get; set; } = new NestSub();
    }
    public class NestSub
    {
        [Category("检测区域")][DisplayName("子区域名称")] public string Name { get; set; } = "入口通道";
        [Category("检测区域")][DisplayName("通道")] public NestSub2 Ch { get; set; } = new NestSub2();
        public override string ToString() => Name;
    }
    public class NestSub2
    {
        [Category("检测区域")][DisplayName("通道号")] public int Id { get; set; } = 1;
        [Category("检测区域")][DisplayName("图像参数")] public NestSub3 P { get; set; } = new NestSub3();
        public override string ToString() => $"通道 {Id}";
    }
    public class NestSub3
    {
        [Category("检测区域")][DisplayName("曝光")][DefaultValue(0.5)] public double Exposure { get; set; } = 0.5;
        [Category("检测区域")][DisplayName("增益")][DefaultValue(1.0)] public double Gain { get; set; } = 1.2;
        public override string ToString() => "图像参数";
    }
}
