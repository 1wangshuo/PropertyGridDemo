using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 标记属性使用集合编辑器编辑（List、数组等）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CollectionEditorAttribute : Attribute
    {
        /// <summary>是否允许添加新项</summary>
        public bool CanAdd { get; set; } = true;

        /// <summary>是否允许删除项</summary>
        public bool CanRemove { get; set; } = true;

        /// <summary>是否允许复制项</summary>
        public bool CanCopy { get; set; } = true;

        /// <summary>是否允许排序（上移/下移）</summary>
        public bool CanSort { get; set; } = true;
    }
}
