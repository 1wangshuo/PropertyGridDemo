using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PropertyGridLib.Controls;

namespace PropertyGridLib
{
    /// <summary>
    /// PropertyGridPro 扁平列表（类别头 + 属性行压平为单层）的行模板选择器：
    /// 类别头用固定模板，属性行交给宿主的 <see cref="PropertyGridPro.RowTemplate"/>。
    /// </summary>
    /// <remarks>
    /// 为什么不用「隐式 DataTemplate（DataType=PropertyItem）」：
    /// 隐式模板会对 ItemsControl 的<b>所有后代</b>生效。而属性行模板（ProPropertyRowTemplate）
    /// 内部渲染子属性时靠模板查找拿行模板，一旦由隐式模板承载「ContentControl + RowTemplate」，
    /// RowTemplate 解析不到就会回退到该隐式模板自身 —— ContentControl 再套 ContentControl，
    /// 无限递归。实测直接栈溢出（退出码 0xC00000FD / -1073741571），进程起不来。
    /// 用选择器按项类型显式分派，不参与后代的资源查找，递归无从发生。
    /// </remarks>
    public class ProFlatRowTemplateSelector : DataTemplateSelector
    {
        /// <summary>类别头模板（竖条 + 分类名；分类名为空时不渲染）</summary>
        public DataTemplate? CategoryTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is PropertyCategory)
                return CategoryTemplate;

            // 属性行：交给宿主的 RowTemplate，宿主整体替换行模板时同样生效
            return FindAncestor<PropertyGridPro>(container)?.RowTemplate;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : class
        {
            while (current != null)
            {
                if (current is T match) return match;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
