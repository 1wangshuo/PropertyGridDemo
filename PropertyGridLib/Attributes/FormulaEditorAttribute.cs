using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 标记属性支持公式绑定编辑器。
    /// 标记后，属性编辑器下方会额外显示一行公式绑定区（链接图标 + 公式文本框）。
    /// 通过 providerType 参数指定实现了 IFormulaTreeProvider 接口的类型，
    /// 用法类似 [TypeConverter(typeof(xxx))]。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FormulaEditorAttribute : Attribute
    {
        /// <summary>
        /// 公式树数据提供者类型（必须实现 IFormulaTreeProvider 接口）
        /// </summary>
        public Type ProviderType { get; }

        /// <summary>
        /// 标记属性支持公式绑定，并指定树数据提供者类型。
        /// </summary>
        /// <param name="providerType">实现了 IFormulaTreeProvider 接口的类型</param>
        public FormulaEditorAttribute(Type providerType = null)
        {
            ProviderType = providerType;
        }
    }
}
