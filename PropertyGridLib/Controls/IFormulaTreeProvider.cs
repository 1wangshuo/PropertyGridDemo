using System.Collections.Generic;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 公式绑定树数据提供者接口，由宿主应用实现。
    /// 当用户点击公式绑定区的链接图标时，调用此接口获取树形数据。
    /// </summary>
    public interface IFormulaTreeProvider
    {
        /// <summary>
        /// 获取公式绑定树数据。
        /// </summary>
        /// <param name="propertyItem">当前属性项</param>
        /// <returns>树节点列表</returns>
        List<FormulaTreeNode> GetFormulaTree(PropertyItem propertyItem);
    }
}
