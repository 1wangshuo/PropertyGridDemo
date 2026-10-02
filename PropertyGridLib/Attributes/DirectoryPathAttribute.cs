using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 标记属性为目录路径选择器：单元格渲染成「文本框 + … 浏览按钮」，点击弹出文件夹树选择框，
    /// 选中的目录直接写回属性值（属性 Value 的 setter → 宿主对象立刻拿到新路径）。
    ///
    /// 用法（任何 string 类型的目录类属性）：
    /// <code>
    /// [DirectoryPath(Title = "选择该 PLC 的记录目录")]
    /// public string RecordDirectory { get; set; }
    /// </code>
    /// 只读属性（<c>[ReadOnly(true)]</c> / 只有 getter）按钮自动禁用，仍可作为"当前目录"展示。
    /// 打开时优先定位到当前值对应的已有目录；值不存在则退回 <see cref="InitialDirectory"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DirectoryPathAttribute : Attribute
    {
        /// <summary>初始目录（当前值为空或不存在时的兜底起始位置）。</summary>
        public string InitialDirectory { get; set; }

        /// <summary>对话框标题/描述（不写则用本地化默认文案"选择文件夹"，随语言切换）。</summary>
        public string Title { get; set; }
    }
}
