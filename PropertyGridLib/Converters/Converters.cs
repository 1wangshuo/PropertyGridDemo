using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using PropertyGridLib.Controls;

namespace PropertyGridLib.Converters
{
    /// <summary>bool 转 Visibility</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            bool invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
            if (invert) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    /// <summary>
    /// bool 转 Visibility（未满足条件时返回 Hidden，保留占位空间，避免布局抖动）
    /// 用于复位按钮等「常驻占位」场景：未修改时不可见但仍占据原有空间。
    /// </summary>
    public class BoolToHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            bool invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
            if (invert) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    /// <summary>bool 取反</summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return false;
        }
    }

    /// <summary>
    /// 多行文本的单行预览：取首行，多行时行尾追加「…」。
    /// TextBox/TextBlock 都会把值里的 \n 渲染成真实换行（TextTrimming 只管横向截断），
    /// 「单行预览」必须先把换行剥掉，否则预览框竖向长高撑破行卡片。
    /// </summary>
    public class FirstLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var idx = text.IndexOfAny(new[] { '\r', '\n' });
            if (idx < 0) return text;

            return text.Substring(0, idx).TrimEnd() + " …";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>null 转 Visibility（非null时显示）</summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
            if (invert) isNull = !isNull;
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>属性类型到编辑器可见性的转换器</summary>
    public class PropertyTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            var propType = value as Type;
            if (propType == null) return Visibility.Collapsed;
            var editorType = parameter.ToString();

            bool match = false;
            switch (editorType)
            {
                case "String":
                    match = propType == typeof(string);
                    break;
                case "Boolean":
                    match = propType == typeof(bool) || propType == typeof(bool?);
                    break;
                case "Number":
                    match = propType == typeof(int) || propType == typeof(double) ||
                            propType == typeof(float) || propType == typeof(decimal) ||
                            propType == typeof(long) || propType == typeof(short) ||
                            propType == typeof(byte);
                    break;
                case "Enum":
                    match = propType.IsEnum;
                    break;
                case "DateTime":
                    match = propType == typeof(DateTime) || propType == typeof(DateTime?);
                    break;
            }

            return match ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>自定义编辑器类型到可见性的转换器</summary>
    public class CustomEditorToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            if (value is CustomEditorType editorType)
            {
                string param = parameter.ToString();
                bool match = false;
                switch (param)
                {
                    case "FilePath":
                        match = editorType == CustomEditorType.FilePath;
                        break;
                    case "DirectoryPath":
                        match = editorType == CustomEditorType.DirectoryPath;
                        break;
                    case "Collection":
                        match = editorType == CustomEditorType.Collection;
                        break;
                    case "Expandable":
                        match = editorType == CustomEditorType.Expandable;
                        break;
                    case "Slider":
                        match = editorType == CustomEditorType.Slider;
                        break;
                }
                return match ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>CustomEditorType 为 None 时显示</summary>
    public class IsNoneToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CustomEditorType editorType)
            {
                return editorType == CustomEditorType.None ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible; // null 也视为 None
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>多条件 AND 可见性转换器（所有条件都 Visible 时才 Visible）</summary>
    public class AndVisConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is Visibility vis && vis != Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>数字类型转 Double（用于 Slider 绑定）</summary>
    public class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0.0;
            if (value is double d) return d;
            if (value is int i) return (double)i;
            if (value is float f) return (double)f;
            if (value is long l) return (double)l;
            if (value is decimal m) return (double)m;
            if (value is short s) return (double)s;
            if (value is byte b) return (double)b;
            try { return System.Convert.ToDouble(value); }
            catch { return 0.0; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            if (targetType == typeof(int)) return System.Convert.ToInt32(value);
            if (targetType == typeof(float)) return System.Convert.ToSingle(value);
            if (targetType == typeof(long)) return System.Convert.ToInt64(value);
            if (targetType == typeof(decimal)) return System.Convert.ToDecimal(value);
            if (targetType == typeof(short)) return System.Convert.ToInt16(value);
            if (targetType == typeof(byte)) return System.Convert.ToByte(value);
            if (targetType == typeof(double)) return System.Convert.ToDouble(value);
            return value;
        }
    }

    /// <summary>
    /// DateTime 转换器（用于 DatePicker 绑定）
    /// </summary>
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt) return dt;
            if (value is string s && DateTime.TryParse(s, out var result)) return result;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt) return dt;
            if (value is string s && DateTime.TryParse(s, out var result)) return result;
            return null;
        }
    }

    /// <summary>
    /// 通用对象类型转换器（用于 CheckBox、NumericUpDown 等绑定）
    /// </summary>
    public class ObjectToTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return GetDefaultValue(targetType);
            
            // 如果已经是目标类型，直接返回
            if (targetType.IsInstanceOfType(value)) return value;
            
            try
            {
                // 处理可空类型
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                
                // bool 转换
                if (underlyingType == typeof(bool))
                {
                    if (value is bool b) return b;
                    if (value is string s) return bool.TryParse(s, out var result) && result;
                    return false;
                }
                
                // 数字类型转换
                if (underlyingType == typeof(int))
                {
                    if (value is int i) return i;
                    if (value is double d) return (int)d;
                    if (value is float f) return (int)f;
                    if (value is string s && int.TryParse(s, out var result)) return result;
                    return 0;
                }
                
                if (underlyingType == typeof(double))
                {
                    if (value is double d) return d;
                    if (value is int i) return (double)i;
                    if (value is float f) return (double)f;
                    if (value is string s && double.TryParse(s, out var result)) return result;
                    return 0.0;
                }
                
                if (underlyingType == typeof(float))
                {
                    if (value is float f) return f;
                    if (value is double d) return (float)d;
                    if (value is int i) return (float)i;
                    if (value is string s && float.TryParse(s, out var result)) return result;
                    return 0.0f;
                }
                
                if (underlyingType == typeof(decimal))
                {
                    if (value is decimal m) return m;
                    if (value is double d) return (decimal)d;
                    if (value is int i) return (decimal)i;
                    if (value is string s && decimal.TryParse(s, out var result)) return result;
                    return 0m;
                }
                
                if (underlyingType == typeof(long))
                {
                    if (value is long l) return l;
                    if (value is int i) return (long)i;
                    if (value is string s && long.TryParse(s, out var result)) return result;
                    return 0L;
                }
                
                if (underlyingType == typeof(short))
                {
                    if (value is short s) return s;
                    if (value is int i) return (short)i;
                    if (value is string str && short.TryParse(str, out var result)) return result;
                    return (short)0;
                }
                
                if (underlyingType == typeof(byte))
                {
                    if (value is byte b) return b;
                    if (value is int i) return (byte)i;
                    if (value is string str && byte.TryParse(str, out var result)) return result;
                    return (byte)0;
                }
                
                // 字符串转换
                if (underlyingType == typeof(string))
                {
                    return value.ToString();
                }
                
                // 枚举转换
                if (underlyingType.IsEnum)
                {
                    if (value is string s)
                    {
                        var result = Enum.Parse(underlyingType, s, true);
                        return result;
                    }
                    if (value is int i)
                        return Enum.ToObject(underlyingType, i);
                    return Enum.ToObject(underlyingType, 0);
                }
                
                // DateTime 转换
                if (underlyingType == typeof(DateTime))
                {
                    if (value is DateTime dt) return dt;
                    if (value is string s && DateTime.TryParse(s, out var result)) return result;
                    return DateTime.MinValue;
                }
                
                // 其他类型尝试使用 System.Convert
                return System.Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        private object GetDefaultValue(Type type)
        {
            if (type == typeof(bool)) return false;
            if (type == typeof(int)) return 0;
            if (type == typeof(double)) return 0.0;
            if (type == typeof(float)) return 0.0f;
            if (type == typeof(decimal)) return 0m;
            if (type == typeof(long)) return 0L;
            if (type == typeof(short)) return (short)0;
            if (type == typeof(byte)) return (byte)0;
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(DateTime)) return DateTime.MinValue;
            if (Nullable.GetUnderlyingType(type) != null) return null;
            return Activator.CreateInstance(type);
        }
    }

    /// <summary>
    /// Color / Color? ↔ SolidColorBrush 双向转换器。
    /// HandyControl ColorPicker 绑定 SolidColorBrush，而属性可能是 Color struct。
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            // 已经是 SolidColorBrush，直接返回
            if (value is SolidColorBrush scb) return scb;

            // Color struct → SolidColorBrush（Freeze 防止绑定循环）
            if (value is Color color)
            {
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }

            // Color?（装箱后也是 Color，上面已覆盖）
            // Brush 其他子类 → 取 Color
            if (value is Brush br)
            {
                if (br is SolidColorBrush b) return b;
                // 非 SolidColorBrush 无法反向取 Color，返回 null
                return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                // 目标是可空类型 → null；非可空 → 默认色
                var underlying = Nullable.GetUnderlyingType(targetType);
                if (underlying != null) return null;
                if (targetType == typeof(Color)) return Colors.Black;
                return null;
            }

            if (value is SolidColorBrush scb)
            {
                if (targetType == typeof(Color)) return scb.Color;
                if (targetType == typeof(Color?)) return scb.Color;
                if (typeof(Brush).IsAssignableFrom(targetType)) return scb;
                return scb.Color;
            }

            if (value is Color c)
            {
                if (targetType == typeof(Color) || targetType == typeof(Color?)) return c;
                if (typeof(Brush).IsAssignableFrom(targetType)) return new SolidColorBrush(c);
                return c;
            }

            return value;
        }
    }
}
