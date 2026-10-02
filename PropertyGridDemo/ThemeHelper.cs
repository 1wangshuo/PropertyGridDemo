using System;
using System.Windows;
using HandyControl.Data;

namespace PropertyGridDemo
{
    /// <summary>
    /// 主题切换助手：统一管理 HandyControl 皮肤（SkinType.Default 浅色 / SkinType.Dark 深色）。
    /// 做法是替换应用级资源字典中合并的皮肤字典（App.xaml 里的 SkinDefault.xaml / SkinDark.xaml），
    /// 由于窗口与控件统一使用主题资源键（DynamicResource），切换后无需重建视觉树即可实时换色。
    /// </summary>
    public static class ThemeHelper
    {
        private const string LightSkinUri = "pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml";
        private const string DarkSkinUri = "pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml";

        /// <summary>当前是否为深色皮肤。</summary>
        public static bool IsDark { get; private set; }

        /// <summary>在浅色 / 深色之间切换。</summary>
        public static void Toggle()
        {
            Apply(IsDark ? SkinType.Default : SkinType.Dark);
        }

        /// <summary>应用指定皮肤到应用级资源字典。</summary>
        public static void Apply(SkinType skin)
        {
            var appRes = Application.Current?.Resources;
            if (appRes == null) return;

            var target = skin == SkinType.Dark ? DarkSkinUri : LightSkinUri;

            // 1) 重建 HandyControl 的主题字典：皮肤字典换成目标皮肤，其余（Theme.xaml）用同源新实例替换。
            //    必须换成新实例，否则旧字典里已解析/冻结的画刷会继续沿用上一次的配色，切换不会生效。
            var merged = appRes.MergedDictionaries;
            var replacedSkin = false;
            for (int i = 0; i < merged.Count; i++)
            {
                var current = merged[i];
                if (current is HandyControl.Themes.Theme) continue;   // Theme 字典交给第 2 步同步

                var src = current?.Source?.OriginalString;
                if (src == null || src.IndexOf("/HandyControl;component/Themes/", StringComparison.OrdinalIgnoreCase) < 0) continue;

                if (IsSkinUri(src))
                {
                    if (!string.Equals(src, target, StringComparison.OrdinalIgnoreCase))
                    {
                        merged[i] = NewDictionary(target);
                    }
                    replacedSkin = true;
                }
                else
                {
                    merged[i] = NewDictionary(src);
                }
            }

            if (!replacedSkin)
            {
                merged.Insert(0, NewDictionary(target));
            }

            // 2) 若应用里挂了 HandyControl.Themes.Theme 字典，一并同步 Skin，与 HandyControl 自身机制保持一致
            foreach (var dict in merged)
            {
                if (dict is HandyControl.Themes.Theme theme)
                {
                    try { theme.Skin = skin; }
                    catch { /* 皮肤已由上面的皮肤字典决定，同步失败不影响显示 */ }
                }
            }

            IsDark = skin == SkinType.Dark;
        }

        private static bool IsSkinUri(string uri)
        {
            return uri.IndexOf("/Themes/Skin", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static ResourceDictionary NewDictionary(string uri)
        {
            return new ResourceDictionary { Source = new Uri(uri, UriKind.Absolute) };
        }
    }
}
