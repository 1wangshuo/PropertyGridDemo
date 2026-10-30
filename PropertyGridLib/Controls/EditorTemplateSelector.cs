using System.Windows;
using System.Windows.Controls;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 根据属性类型自动选择对应的编辑器模板
    /// </summary>
    public class EditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }
        public DataTemplate NumberTemplate { get; set; }
        public DataTemplate EnumTemplate { get; set; }
        public DataTemplate DateTimeTemplate { get; set; }
        public DataTemplate FilePathTemplate { get; set; }
        public DataTemplate DirectoryPathTemplate { get; set; }
        public DataTemplate CollectionTemplate { get; set; }
        public DataTemplate DictionaryTemplate { get; set; }
        public DataTemplate SliderTemplate { get; set; }
        public DataTemplate ExpandableTemplate { get; set; }
        public DataTemplate DropDownTemplate { get; set; }
        public DataTemplate MultiSelectTemplate { get; set; }
        public DataTemplate ButtonTemplate { get; set; }
        public DataTemplate ColorTemplate { get; set; }
        /// <summary>布尔开关（ToggleSwitch）编辑器模板，由 [ToggleSwitch] 特性触发</summary>
        public DataTemplate ToggleSwitchTemplate { get; set; }
        /// <summary>多行文本（富文本查看）编辑器模板，由 [MultilineText] 特性触发</summary>
        public DataTemplate MultilineTextTemplate { get; set; }
        /// <summary>范围滑块编辑器模板：属性类型为 RangeValue 时自动启用（[RangeSlider] 可配边界）</summary>
        public DataTemplate RangeSliderTemplate { get; set; }
        /// <summary>链接编辑器模板：字符串值本身是 http/https/ftp/mailto 链接时自动启用（蓝色可点击）</summary>
        public DataTemplate LinkTemplate { get; set; }
        /// <summary>
        /// 图像预览编辑器模板：值本身是图像时启用（ImageSource / 图像文件字节流 /
        /// 经 ImagePreviewRegistry 登记的自定义图像类型，如平台契约的 GenImage）。
        /// </summary>
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PropertyItem propertyItem)
            {
                // ===== 宿主自定义编辑器模板优先级最高 =====
                if (propertyItem.CustomEditorTemplate != null)
                    return propertyItem.CustomEditorTemplate;

                // ===== 值形态：图像 =====
                // 必须排在 CustomEditor 分派之前：自定义图像类型（GenImage 这类 class）
                // 会被 DetectCustomEditor 判成 Expandable（可展开对象），
                // 那样就会命中 ExpandableTemplate、显示成一个可展开的类型名，
                // 永远轮不到下面的图像模板。
                if (ImageTemplate != null && propertyItem.IsImage)
                    return ImageTemplate;

                // 根据 CustomEditor 优先选择
                switch (propertyItem.CustomEditor)
                {
                    case CustomEditorType.FilePath:
                        return FilePathTemplate ?? DefaultTemplate;
                    case CustomEditorType.DirectoryPath:
                        return DirectoryPathTemplate ?? DefaultTemplate;
                    case CustomEditorType.Collection:
                        return CollectionTemplate ?? DefaultTemplate;
                    case CustomEditorType.Dictionary:
                        return DictionaryTemplate ?? CollectionTemplate ?? DefaultTemplate;
                    case CustomEditorType.Slider:
                        return SliderTemplate ?? DefaultTemplate;
                    case CustomEditorType.Expandable:
                        return ExpandableTemplate ?? DefaultTemplate;
                    case CustomEditorType.DropDown:
                        return DropDownTemplate ?? DefaultTemplate;
                    case CustomEditorType.MultiSelect:
                        return MultiSelectTemplate ?? DefaultTemplate;
                    case CustomEditorType.Button:
                        return ButtonTemplate ?? DefaultTemplate;
                    case CustomEditorType.Color:
                        return ColorTemplate ?? DefaultTemplate;
                    case CustomEditorType.ToggleSwitch:
                        // [ToggleSwitch] 特性：开关编辑器；模板缺失时回退到默认 bool 编辑器，保证不空白
                        return ToggleSwitchTemplate ?? BooleanTemplate ?? DefaultTemplate;
                    case CustomEditorType.MultilineText:
                        // [MultilineText] 特性：多行文本编辑器；模板缺失时回退到默认 string 编辑器
                        return MultilineTextTemplate ?? StringTemplate ?? DefaultTemplate;
                    case CustomEditorType.RangeSlider:
                        // RangeValue 类型：范围滑块；模板缺失时回退到默认只读文本，保证不空白
                        return RangeSliderTemplate ?? DefaultTemplate;
                }

                // 根据属性类型选择（使用 EffectivePropertyType 以支持 FormulaBound<T>）
                var propertyType = propertyItem.EffectivePropertyType;

                // 字符串值本身是链接（http/https/ftp/mailto）→ 蓝色可点击文本，点击用默认程序打开。
                // 放在类型分派之前：链接是"值的形态"，与属性声明类型无关（string 才有意义）。
                if (propertyType == typeof(string) && propertyItem.IsLink)
                    return LinkTemplate ?? StringTemplate ?? DefaultTemplate;

                if (propertyType == typeof(string))
                    return StringTemplate ?? DefaultTemplate;

                if (propertyType == typeof(bool))
                    return BooleanTemplate ?? DefaultTemplate;

                if (propertyType == typeof(int) || propertyType == typeof(double) ||
                    propertyType == typeof(float) || propertyType == typeof(decimal) ||
                    propertyType == typeof(long) || propertyType == typeof(short) ||
                    propertyType == typeof(byte))
                    return NumberTemplate ?? DefaultTemplate;

                if (propertyType.IsEnum)
                    return EnumTemplate ?? DefaultTemplate;

                if (propertyType == typeof(System.DateTime) || propertyType == typeof(System.DateTime?))
                    return DateTimeTemplate ?? DefaultTemplate;

                // 复杂对象使用 DefaultTemplate（显示为只读文本）
                return DefaultTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
