using System;
using System.Globalization;
using System.Windows.Data;

namespace PropertyGridLib.Converters
{
    /// <summary>
    /// 字体大小偏移转换器：基于绑定的 FontSize 加上偏移量（ConverterParameter），保持文本层级。
    /// 参数示例：0 表示跟随，-1 表示小一号，+1 表示大一号。
    /// </summary>
    public class FontSizeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double baseSize = 12.0;
            if (value is double d && d > 0)
                baseSize = d;

            double offset = 0;
            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out offset);
            }

            return Math.Max(6, baseSize + offset);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
