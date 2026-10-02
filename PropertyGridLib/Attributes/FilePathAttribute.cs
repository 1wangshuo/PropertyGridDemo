using System;
using System.Linq;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 标记属性为文件路径选择器，点击浏览按钮选择文件。
    /// 用法：[FilePath("*.json", "*.xml", "*.txt")] 支持多后缀筛选
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FilePathAttribute : Attribute
    {
        /// <summary>文件后缀列表（如 ["*.json", "*.xml"]）</summary>
        public string[] Filters { get; }

        /// <summary>
        /// 文件过滤器字符串（自动从 Filters 拼接，如 "*.json;*.xml"）。
        /// 兼容旧代码直接赋值，但优先使用构造函数传参。
        /// </summary>
        public string Filter
        {
            get => _explicitFilter ?? (Filters != null && Filters.Length > 0
                ? string.Join(";", Filters)
                : null);
            set => _explicitFilter = value;
        }
        private string _explicitFilter;

        /// <summary>初始目录</summary>
        public string InitialDirectory { get; set; }

        /// <summary>对话框标题</summary>
        public string Title { get; set; }

        /// <summary>是否允许多选</summary>
        public bool Multiselect { get; set; }

        /// <summary>
        /// 指定文件后缀，支持多个：[FilePath("*.json", "*.xml", "*.txt")]
        /// </summary>
        public FilePathAttribute(params string[] filters)
        {
            Filters = filters;
            // Title 保持 null：浏览对话框标题由库按语言提供（zh「选择文件」/ en「Select File」）；
            // 宿主可显式设置 Title 覆盖（经 TextResolver 翻译）。
        }
    }
}
