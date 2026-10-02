using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 标记属性为多选列表编辑器。
    /// 用法：[MultiSelect(typeof(MyProvider))]
    /// 属性类型应为 List&lt;string&gt;，Provider 动态返回可选项。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MultiSelectAttribute : Attribute
    {
        /// <summary>多选数据提供者类型（实现 IMultiSelectProvider）</summary>
        public Type ProviderType { get; }

        public MultiSelectAttribute(Type providerType)
        {
            ProviderType = providerType;
        }
    }
}
