using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PropertyGridLib.Localization
{
    /// <summary>
    /// XAML 标记扩展：{loc:Localized Key} 
    /// 绑定到 LocalizationProxy 索引器，语言切换时自动刷新。
    /// </summary>
    public class LocalizedExtension : MarkupExtension
    {
        public string Key { get; set; }

        public LocalizedExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding
            {
                Source = LocalizationProxy.Instance,
                Path = new PropertyPath($"[{Key}]"),
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
