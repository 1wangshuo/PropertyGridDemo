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

        /// <summary>表单式（<see cref="ProLayout.Section"/>）的类别头模板：左侧大标题 + 右侧延伸分隔线</summary>
        public DataTemplate? SectionCategoryTemplate { get; set; }

        /// <summary>表单式的属性行模板：名称 → 说明 → 编辑器，上下排布</summary>
        public DataTemplate? SectionRowTemplate { get; set; }

        /// <summary>表单式的布尔勾选项模板：复选框 + 名称 + 说明</summary>
        public DataTemplate? CheckRowTemplate { get; set; }

        /// <summary>
        /// 形态回退值：正常情况下直接从祖先 <see cref="PropertyGridPro"/> 读 <see cref="PropertyGridPro.Layout"/>，
        /// 只有选择器被单独使用（不在 Pro 里）时才需要显式设置。
        /// </summary>
        public ProLayout Layout { get; set; } = ProLayout.Card;

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            var pro = FindAncestor<PropertyGridPro>(container);
            var layout = pro?.Layout ?? Layout;
            var section = layout == ProLayout.Section;

            if (item is PropertyCategory)
                return (section ? SectionCategoryTemplate : null) ?? CategoryTemplate;

            // 表单式 + 布尔属性 → 勾选项形态（复选框 + 名称 + 说明）。
            // 卡片式里布尔仍是一行下拉：窄卡片里勾选项反而更占高度。
            // 注意 bool 默认的 CustomEditor 是 DropDown（True/False 下拉），
            // 只有宿主用特性显式指定了别的编辑器（开关/滑块等）时才让位。
            if (section &&
                CheckRowTemplate != null &&
                item is PropertyItem pi &&
                pi.EffectivePropertyType == typeof(bool) &&
                (pi.CustomEditor == CustomEditorType.None || pi.CustomEditor == CustomEditorType.DropDown))
            {
                return CheckRowTemplate;
            }

            if (section && SectionRowTemplate != null)
                return SectionRowTemplate;

            // 属性行：交给宿主的 RowTemplate，宿主整体替换行模板时同样生效
            return pro?.RowTemplate;
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
