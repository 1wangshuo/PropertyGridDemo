using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 标记"关键属性"：在属性面板上打一个强调标记。
    ///
    /// 呈现随形态而变：
    /// * 基础 <c>PropertyGrid</c>：属性名旁一个强调色小圆点（对应 HandyControl 标题上那颗红点的语义）；
    /// * <c>PropertyGridPro</c> 表单式（<see cref="PropertyGridLib.ProLayout.Section"/>）：
    ///   该属性块左侧出现一段强调色竖线；未标记的属性没有竖线。
    ///
    /// 用法：
    ///   [Highlight]                                  // 只要有标记，不需要提示文字
    ///   [Highlight(Tip = "改这里会影响产线节拍")]      // 标记 + 悬停提示
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HighlightAttribute : Attribute
    {
        /// <summary>悬停标记时显示的提示（为空则标记不挂 ToolTip）。</summary>
        public string Tip { get; set; }
    }
}
