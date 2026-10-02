using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 多行文本（富文本查看）编辑器特性。
    /// <para>
    /// 标注在 <c>string</c> 属性上时，属性行编辑器切换为「单行只读预览 + “…”按钮」样式：
    /// 点击按钮弹出多行富文本编辑窗口（自动换行 + 允许回车换行 + 独立滚动），
    /// 适合文本较长、需要查看/编辑多行内容的属性（如备注、说明、代码片段等）。
    /// </para>
    /// <para>
    /// 行内仍保持单行等高，不破坏属性面板虚拟化的行高估算（滚动条不抖动）。
    /// 仅对 <c>string</c> 类型属性生效，其它类型标注后被忽略。
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// [MultilineText]
    /// public string Remarks { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MultilineTextAttribute : Attribute
    {
    }
}
