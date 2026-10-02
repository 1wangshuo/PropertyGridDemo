using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PropertyGridDemo.Models
{
    /// <summary>
    /// 串口列表 TypeConverter 演示
    /// </summary>
    public class ComPortListConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var ports = new List<string> { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6" };
            return new StandardValuesCollection(ports);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
    }
}
