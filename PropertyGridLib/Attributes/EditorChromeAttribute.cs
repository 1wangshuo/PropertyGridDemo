using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 编辑器"装饰件"的开关：控制属性行右侧那些附属小件（重置按钮、类型提示等）。
    ///
    /// 为什么要特性化：这些装饰件此前只有"全局开关"（PropertyGrid.ShowTypeHint），
    /// 但实际需求是**逐个属性**定的 —— 比如「停留秒数」希望有个重置按钮，
    /// 「预置位名称」不希望有；某个数值不需要 `(Int32)` 提示，另一个又需要。
    /// 打在属性上比让宿主自己拼模板省事得多，也不会污染全局。
    ///
    /// 用法：
    /// <code>
    /// [EditorChrome(ShowReset = false)]                              // 这一行不要重置按钮
    /// [EditorChrome(ResetText = "默认", ResetToolTip = "恢复出厂值")]   // 改名 + 换提示
    /// [EditorChrome(ShowTypeHint = true)]                            // 单独打开类型提示
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class EditorChromeAttribute : Attribute
    {
        /// <summary>是否显示重置按钮（默认 true：值被改过时出现）。</summary>
        public bool ShowReset { get; set; } = true;

        /// <summary>重置按钮上的文字（默认 "R"）。改成"默认""复位"都行，两三个字以内最好看。</summary>
        public string ResetText { get; set; }

        /// <summary>重置按钮的悬停提示（默认取本地化的「重置为默认值」）。</summary>
        public string ResetToolTip { get; set; }

        /// <summary>
        /// 是否显示类型提示（`(Int32)` 那类）。
        /// 留空（null）= 跟随 <c>PropertyGrid.ShowTypeHint</c> 的全局值；
        /// 显式 true/false 则只对这一行生效。
        /// </summary>
        public bool? ShowTypeHint { get; set; }
    }
}
