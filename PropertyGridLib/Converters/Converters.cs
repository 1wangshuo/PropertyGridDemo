using System;
using System.Collections.Concurrent;
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
        /// <summary>
        /// 颜色 → 冻结画刷的全局缓存。
        /// 原来每次 Convert 都 <c>new SolidColorBrush(color)</c> 再 Freeze：颜色块/可展开行的值一变就换新实例，
        /// 旧实例在紧随的布局里被 WPF 从组合通道上释放，实测**连点「重置所有属性」第二次必崩**
        /// （SolidColorBrush.ReleaseOnChannelCore → COMException 0x8000FFFF → Visual.DisconnectAttachedResource
        /// → UIElement.RenderClose → Arrange → ContextLayoutManager.UpdateLayout）。
        /// 冻结后的 Freezable 不可变、可被多处共享，按颜色缓存同一实例即可消除「每次求值新建/释放」这一循环，
        /// 同时保住了「颜色相同 → 绑定值不变 → 连通知都不产生」的短路效果。
        /// </summary>
        private static readonly ConcurrentDictionary<Color, SolidColorBrush> FrozenBrushCache
            = new ConcurrentDictionary<Color, SolidColorBrush>();

        /// <summary>取某个颜色的共享冻结画刷（Color 是 struct，作键无装箱歧义）</summary>
        internal static SolidColorBrush GetFrozenBrush(Color color)
            => FrozenBrushCache.GetOrAdd(color, c =>
            {
                var brush = new SolidColorBrush(c);
                brush.Freeze();
                return brush;
            });

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            // 已经是 SolidColorBrush，直接返回
            if (value is SolidColorBrush scb) return scb;

            // Color struct → 共享的冻结 SolidColorBrush
            if (value is Color color)
                return GetFrozenBrush(color);

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

    /// <summary>
    /// 取长度值的指定比例，供 Small 紧凑布局的标题列做"Auto 但封顶"：
    /// 标题列本身是 Auto（短标题不占多余宽度，编辑器多吃空间），
    /// 但要给标题一个上限，否则长标题会把编辑器挤到只剩几十像素。
    /// 参数为比例（"0.5" = 一半）；参数缺省或非数字时按 0.5 处理。
    /// 传入长度无效（0 / NaN，布局尚未定型）时返回 Infinity = 不封顶，
    /// 避免"第一轮按 0 封顶 → 第二轮再放开"的反复测量。
    /// </summary>
    public class RatioLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double length) || double.IsNaN(length) || length <= 0)
                return double.PositiveInfinity;

            var ratio = 0.5;
            if (parameter is string text &&
                double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                ratio = parsed;

            return Math.Max(0, length * ratio);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// 表单式分类列宽：取「期望列宽」和「面板宽 × 比例」里较小的那个。
    ///
    /// 为什么要它：分类列是固定像素（默认 152），面板被拖窄到 250 左右时它要占掉 65%，
    /// 右侧属性只剩几十像素 —— 属性名被省略成两个字、说明文字竖着一个个排，
    /// 看上去就像"内容全空"。这里让它随面板收窄，宁可分类名省略也别挤死内容区。
    ///
    /// values[0] = 面板可用宽度，values[1] = 期望列宽；parameter = 最大占比（默认 0.32）。
    /// </summary>
    public class MinRatioLengthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return double.NaN;

            var preferred = values[1] is double d && !double.IsNaN(d) && d > 0 ? d : 152d;

            var ratio = 0.32;
            if (parameter is string text &&
                double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
                ratio = parsed;

            // 面板还没排版（宽度 0 / NaN）时先用期望值，避免第一轮算出 0 再重排
            if (!(values[0] is double available) || double.IsNaN(available) || available <= 0) return preferred;

            return Math.Max(0, Math.Min(available * ratio, preferred));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
