using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PropertyGridLib
{
    /// <summary>
    /// 属性面板的"图像预览"登记表：把任意自定义图像类型（如平台契约里的
    /// <c>TeamPLT.Contracts.Imaging.GenImage</c>）映射成可显示的 <see cref="ImageSource"/>。
    ///
    /// 为什么要登记表而不是直接支持：
    /// 平台契约层是纯 BCL（不能引用 WPF），<c>GenImage</c> 只有一个 <c>byte[] Data</c> +
    /// <c>PixelFormat</c> 枚举；把它画出来必须由**能引用 WPF 的一侧**（插件 / 扩展 / 宿主）
    /// 提供像素 → 位图的转换。登记表就是这个挂载点，属性面板本身对图像类型零依赖。
    ///
    /// 用法（宿主启动时注册一次）：
    /// <code>
    /// ImagePreviewRegistry.Register(
    ///     typeof(GenImage),
    ///     v => ToBitmapSource((GenImage)v),
    ///     v => { var g = (GenImage)v; return $"{g.Width} × {g.Height}  {g.Format}"; });
    /// </code>
    ///
    /// 内置识别（无需注册）：<see cref="ImageSource"/>（含 BitmapImage / WriteableBitmap）、
    /// 以及 PNG / JPG / BMP / GIF / TIFF 编码的 <c>byte[]</c>。
    /// </summary>
    public static class ImagePreviewRegistry
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<Type, Func<object, ImageSource>> Factories =
            new Dictionary<Type, Func<object, ImageSource>>();
        private static readonly Dictionary<Type, Func<object, string>> Infos =
            new Dictionary<Type, Func<object, string>>();

        /// <summary>
        /// 登记一种图像类型：<paramref name="factory"/> 负责把值转成可显示位图（返回 null 表示这张图暂时画不出来）；
        /// <paramref name="info"/> 可选，返回一行说明（尺寸 / 格式 / 帧号等），显示在缩略图旁边。
        /// 重复登记同类型 = 覆盖（幂等，扩展热重载时不必先反注册）。
        /// </summary>
        public static void Register(Type type, Func<object, ImageSource> factory, Func<object, string> info = null)
        {
            if (type == null || factory == null) return;
            lock (Gate)
            {
                Factories[type] = factory;
                if (info != null) Infos[type] = info;
                else Infos.Remove(type);
            }
        }

        /// <summary>反注册（插件卸载时调用，避免卸载后还指向已卸载程序集里的转换函数）。</summary>
        public static void Unregister(Type type)
        {
            if (type == null) return;
            lock (Gate)
            {
                Factories.Remove(type);
                Infos.Remove(type);
            }
        }

        /// <summary>
        /// 这个值能不能当图像预览。
        /// </summary>
        public static bool CanPreview(object value)
        {
            if (value == null) return false;
            if (value is ImageSource) return true;
            if (value is byte[] bytes) return LooksLikeImageFile(bytes);
            if (value is IImagePreviewSource) return true;

            return CanPreviewType(value.GetType());
        }

        /// <summary>
        /// 这个**声明类型**能不能当图像预览（不看具体值）。
        /// 用于"值为 null"的情况：属性声明的是图像类型、当前还没图时，
        /// 面板应该显示"无预览"占位，而不是退回成一个空的可展开行。
        /// </summary>
        public static bool CanPreviewType(Type type)
        {
            if (type == null) return false;
            if (typeof(ImageSource).IsAssignableFrom(type)) return true;
            if (typeof(IImagePreviewSource).IsAssignableFrom(type)) return true;
            if (type == typeof(byte[])) return true;

            lock (Gate)
            {
                if (Factories.ContainsKey(type)) return true;
                // 登记的是基类/接口时也认（例如登记 IFrame 就覆盖所有实现类）
                foreach (var kv in Factories)
                {
                    if (kv.Key.IsAssignableFrom(type)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 取值：成功返回可显示位图并给出说明文字；失败返回 null（面板只显示"无法预览"，不抛异常）。
        /// </summary>
        public static ImageSource Create(object value, out string info)
        {
            info = null;
            if (value == null) return null;

            try
            {
                if (value is IImagePreviewSource self)
                {
                    info = SafeInfo(self);
                    return self.GetPreviewImage();
                }

                if (value is ImageSource direct) return direct;

                if (value is byte[] bytes) return TryDecodeImageFile(bytes);

                var type = value.GetType();
                Func<object, ImageSource> factory = null;
                Func<object, string> infoFactory = null;
                lock (Gate)
                {
                    if (!Factories.TryGetValue(type, out factory))
                    {
                        foreach (var kv in Factories)
                        {
                            if (!kv.Key.IsAssignableFrom(type)) continue;
                            factory = kv.Value;
                            Infos.TryGetValue(kv.Key, out infoFactory);
                            break;
                        }
                    }
                    else
                    {
                        Infos.TryGetValue(type, out infoFactory);
                    }
                }

                if (factory == null) return null;
                var image = factory(value);
                if (infoFactory != null)
                {
                    try { info = infoFactory(value); } catch { info = null; }
                }
                return image;
            }
            catch
            {
                // 自定义类型的转换函数不可靠（像素格式不支持 / 数据有问题）：面板不能因此崩
                return null;
            }
        }

        private static string SafeInfo(IImagePreviewSource self)
        {
            try { return self.GetImageInfo(); } catch { return null; }
        }

        /// <summary>字节流是否像一份图像文件（只查文件头，不解码，够便宜）。</summary>
        private static bool LooksLikeImageFile(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 8) return false;
            // PNG
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;
            // JPG
            if (bytes[0] == 0xFF && bytes[1] == 0xD8) return true;
            // BMP
            if (bytes[0] == 0x42 && bytes[1] == 0x4D) return true;
            // GIF
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46) return true;
            // TIFF
            if ((bytes[0] == 0x49 && bytes[1] == 0x49) || (bytes[0] == 0x4D && bytes[1] == 0x4D)) return true;
            return false;
        }

        private static ImageSource TryDecodeImageFile(byte[] bytes)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;   // 立刻解码，不占着流
                image.StreamSource = new MemoryStream(bytes, writable: false);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 让"自定义图像类型"自己提供预览（能引用 WPF 的类型首选这条路，无需在宿主里登记）。
    /// 契约层类型（如 GenImage）不能引用 WPF，改用 <see cref="ImagePreviewRegistry.Register"/>。
    /// </summary>
    public interface IImagePreviewSource
    {
        /// <summary>返回可显示的位图；暂时没有画面时返回 null（面板显示"无预览"）。</summary>
        ImageSource GetPreviewImage();

        /// <summary>缩略图旁的一行说明（尺寸 / 格式等），可为 null。</summary>
        string GetImageInfo();
    }
}
