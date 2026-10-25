using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PropertyGridLib;
using PropertyGridLib.Attributes;

namespace PropertyGridDemo
{
    /// <summary>
    /// 新版形态展示：工具条/分类头隐藏、无分类平铺、Pro 表单式（VS 2026 选项页）、链接属性。
    /// 全部是控件本身的开关，不涉及宿主改造。
    /// </summary>
    public partial class FormLayoutDemoWindow : HandyControl.Controls.Window
    {
        /// <summary>
        /// 把自定义图像类型登记给属性面板（宿主侧一次性动作）：
        /// 平台里换成 typeof(GenImage) + 你们插件里的 GenImage→BitmapSource 转换即可。
        /// </summary>
        static FormLayoutDemoWindow()
        {
            ImagePreviewRegistry.Register(
                typeof(DemoGenImage),
                v => ((DemoGenImage)v).ToBitmapSource(),
                v =>
                {
                    var g = (DemoGenImage)v;
                    return $"{g.Width} × {g.Height}  {g.Format}  {g.Bytes.Length / 1024.0:F0} KB";
                });
        }

        public FormLayoutDemoWindow()
        {
            InitializeComponent();

            var model = new FormDemoObject();
            grid.SelectedObject = model;
            proGrid.SelectedObject = model;

            Shape_Changed(null, null);
        }

        /// <summary>形态开关：四个开关同时作用于左右两个网格，方便直接对比。</summary>
        private void Shape_Changed(object? sender, RoutedEventArgs? e)
        {
            if (grid == null || proGrid == null) return;

            grid.ShowSearchBar = chkSearch.IsChecked == true;
            grid.ShowExpandCollapse = chkExpand.IsChecked == true;
            grid.ShowCategoryHeaders = chkCategory.IsChecked == true;

            proGrid.ShowSearchBar = chkSearch.IsChecked == true;
            proGrid.ShowCategoryHeaders = chkCategory.IsChecked == true;
            proGrid.ShowHeader = chkHeader.IsChecked == true;
            proGrid.Layout = chkSection.IsChecked == true ? ProLayout.Section : ProLayout.Card;

            // 底部属性详情栏：两个开关分开控制（默认值不同 —— 普通版显示、Pro 不显示）
            grid.ShowDescription = chkDesc.IsChecked == true;
            proGrid.ShowDescription = chkProDesc.IsChecked == true;

            // 顶部标题区：普通版新增 ShowHeader（与 Pro 版同名属性，语义一致）
            grid.ShowHeader = chkHeader.IsChecked == true;
        }
    }

    /// <summary>展示用模型：分类 + 布尔项 + 链接属性 + 两种图像（自定义类型 / 位图）。</summary>
    public class FormDemoObject
    {
        [Category("I. 基本信息")]
        [DisplayName("项目名称")]
        [Description("解决方案的显示名称，出现在标题栏与打包文件名里。")]
        // 这一行不要重置按钮（本就没有"默认值"可回退）
        [EditorChrome(ShowReset = false)]
        public string ProjectName { get; set; } = "TeamPLT.PropertyGrid";

        [Category("I. 基本信息")]
        [DisplayName("输出类型")]
        [Description("生成的程序集类型。")]
        public OutputKind OutputType { get; set; } = OutputKind.Library;

        [Category("II. 发布")]
        [DisplayName("版本号")]
        [Description("语义化版本，打包时写入 manifest。")]
        // 关键属性：基础网格标题前出现强调色圆点，表单式属性块左侧出现强调色竖线
        [Highlight(Tip = "改这里会影响打包文件名与升级判定")]
        // 重置按钮改名 + 换悬停文案（默认是 "R" / 「重置为默认值」）
        [EditorChrome(ResetText = "默认", ResetToolTip = "恢复出厂版本号")]
        public string Version { get; set; } = "1.7.0";

        [Category("II. 发布")]
        [DisplayName("发布日期")]
        [Description("留空表示尚未发布。")]
        public DateTime ReleaseDate { get; set; } = new DateTime(2026, 10, 21);

        [Category("III. 构建选项")]
        [DisplayName("生成引用程序集")]
        [Description("生成包含项目公共 API 的引用程序集。")]
        // 关键属性（勾选项也能带标记）
        [PropertyGridLib.Attributes.Highlight]
        public bool ProduceReferenceAssembly { get; set; } = true;

        [Category("III. 构建选项")]
        [DisplayName("生成 API 文档文件")]
        [Description("生成包含 API 文档的 XML 文件。")]
        public bool ProduceDocumentation { get; set; }

        [Category("III. 构建选项")]
        [DisplayName("启用增量编译")]
        [Description("仅重新编译发生变化的文件，缩短迭代时间。")]
        public bool IncrementalBuild { get; set; } = true;

        [Category("IV. 在线资源")]
        [DisplayName("项目主页")]
        [Description("字符串值是链接时自动变成可点击文本，无需额外特性；点击用默认浏览器打开。")]
        public string HomePage { get; set; } = "https://teamaas.page.coze.site/";

        [Category("IV. 在线资源")]
        [DisplayName("反馈邮箱")]
        [Description("mailto: 同样识别为链接。")]
        public string FeedbackMail { get; set; } = "mailto:support@teamaas.example";

        // ── 图像：两种接法 ──
        // ①自定义图像类型（模拟平台契约的 GenImage：宽高 + 像素字节 + 格式），
        //   由宿主在 ImagePreviewRegistry 里登记转换函数后即可预览；
        // ②直接给 WPF 位图（BitmapSource），内置识别，不需要任何登记。
        [Category("V. 图像")]
        [DisplayName("采集样图（自定义类型）")]
        [Description("自定义图像类型，经 ImagePreviewRegistry 登记后渲染缩略图，点击看大图。")]
        public DemoGenImage SampleImage { get; set; } = new DemoGenImage(320, 240);

        [Category("V. 图像")]
        [DisplayName("预览位图（BitmapSource）")]
        [Description("WPF 位图内置识别，无需登记。")]
        public BitmapSource SampleBitmap => DemoGenImage.SharedBitmap;
    }

    /// <summary>
    /// 模拟平台契约里的 <c>TeamPLT.Contracts.Imaging.GenImage</c>：
    /// 纯 BCL 的宽高 + 像素字节 + 格式标识，自己不知道 WPF 的存在。
    /// 真正接入时把 <see cref="ToBitmapSource"/> 里的几行搬到宿主侧登记即可。
    /// </summary>
    public sealed class DemoGenImage
    {
        public DemoGenImage(int width, int height)
        {
            Width = width;
            Height = height;
            Bytes = BuildBgr24(width, height);
        }

        public int Width { get; }
        public int Height { get; }
        public string Format => "Bgr24";
        public byte[] Bytes { get; }

        public BitmapSource ToBitmapSource() =>
            BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Bgr24, null, Bytes, Width * 3);

        /// <summary>一张能看出"确实是图"的样图：横向渐变 + 中间一个实心圆。</summary>
        private static byte[] BuildBgr24(int w, int h)
        {
            var buf = new byte[w * h * 3];
            var cx = w / 2.0;
            var cy = h / 2.0;
            var r = Math.Min(w, h) * 0.3;

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var i = (y * w + x) * 3;
                    var dx = x - cx;
                    var dy = y - cy;
                    if (dx * dx + dy * dy <= r * r)
                    {
                        buf[i] = 60; buf[i + 1] = 200; buf[i + 2] = 250;      // BGR：橙黄
                    }
                    else
                    {
                        buf[i] = (byte)(40 + 180.0 * y / h);                  // B
                        buf[i + 1] = (byte)(60 + 120.0 * x / w);              // G
                        buf[i + 2] = (byte)(90 + 60.0 * y / h);               // R
                    }
                }
            }
            return buf;
        }

        private static BitmapSource _shared;

        /// <summary>给"内置识别"演示用的位图（只生成一次）。</summary>
        public static BitmapSource SharedBitmap =>
            _shared ??= new DemoGenImage(280, 200).ToBitmapSource();
    }

    public enum OutputKind
    {
        Library,
        Console,
        WinExe
    }
}
