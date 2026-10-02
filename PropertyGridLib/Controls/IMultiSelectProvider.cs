using System.Collections.Generic;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 多选列表数据提供者接口，由宿主应用实现。
    /// </summary>
    public interface IMultiSelectProvider
    {
        /// <summary>
        /// 获取所有可选项
        /// </summary>
        List<string> GetAvailableItems(PropertyItem propertyItem);
    }
}
