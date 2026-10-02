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
        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PropertyItem propertyItem)
            {
                // ===== 宿主自定义编辑器模板优先级最高 =====
                if (propertyItem.CustomEditorTemplate != null)
                    return propertyItem.CustomEditorTemplate;

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
                }

                // 根据属性类型选择（使用 EffectivePropertyType 以支持 FormulaBound<T>）
                var propertyType = propertyItem.EffectivePropertyType;

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
