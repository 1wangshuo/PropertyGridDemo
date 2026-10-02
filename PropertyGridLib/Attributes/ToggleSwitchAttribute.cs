using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 布尔开关（ToggleSwitch）编辑器特性。
    /// <para>
    /// 标注在 <c>bool</c> 属性上时，属性行编辑器切换为「滑块式开关 + 状态文字」的 ToggleSwitch 样式
    /// （基于 HandyControl 的开关样式 ToggleButtonSwitch：圆角轨道 + 圆形滑块，旁侧显示开/关状态文字）；
    /// 未标注的 <c>bool</c> 属性保持原有显示（默认布尔编辑器，不受影响）。
    /// </para>
    /// <para>
    /// 仅对 <c>bool</c> 类型属性生效，其它类型标注后被忽略。
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// [ToggleSwitch]                                  // 默认状态文字随库语言（中文「开 / 关」、英文「ON / OFF」）
    /// [ToggleSwitch("已启用", "已禁用")]               // 自定义状态文字（可经宿主 TextResolver 翻译）
    /// [ToggleSwitch(OnText = "", OffText = "")]        // 只显示开关，不显示文字
    /// public bool IsEnabled { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ToggleSwitchAttribute : Attribute
    {
        /// <summary>
        /// 打开（true）状态时显示的文字。null（默认）= 用库内置文字（随语言切换：中文「开」、英文「ON」）；
        /// 空字符串 = 不显示文字；自定义文字会经过全局翻译钩子（TextResolver）。
        /// </summary>
        public string OnText { get; set; }

        /// <summary>
        /// 关闭（false）状态时显示的文字。null（默认）= 用库内置文字（随语言切换：中文「关」、英文「OFF」）；
        /// 空字符串 = 不显示文字；自定义文字会经过全局翻译钩子（TextResolver）。
        /// </summary>
        public string OffText { get; set; }

        /// <summary>
        /// 使用默认状态文字（开 / 关）。
        /// </summary>
        public ToggleSwitchAttribute()
        {
        }

        /// <summary>
        /// 使用自定义状态文字。
        /// </summary>
        /// <param name="onText">打开状态文字</param>
        /// <param name="offText">关闭状态文字</param>
        public ToggleSwitchAttribute(string onText, string offText)
        {
            OnText = onText;
            OffText = offText;
        }
    }
}
