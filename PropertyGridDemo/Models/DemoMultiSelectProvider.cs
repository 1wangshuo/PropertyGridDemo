using System.Collections.Generic;
using PropertyGridLib.Controls;

namespace PropertyGridDemo.Models
{
    /// <summary>
    /// 演示多选列表数据提供者
    /// </summary>
    public class DemoMultiSelectProvider : IMultiSelectProvider
    {
        public List<string> GetAvailableItems(PropertyItem propertyItem)
        {
            return new List<string>
            {
                "选项A", "选项B", "选项C", "选项D", "选项E",
                "选项F", "选项G", "选项H"
            };
        }
    }
}
