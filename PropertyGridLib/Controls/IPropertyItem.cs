using System;
using System.Windows.Input;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 属性项接口（PropertyItem 和 DictionaryPropertyItem 都实现此接口）。
    /// </summary>
    public interface IPropertyItem
    {
        /// <summary>属性名称</summary>
        string Name { get; }

        /// <summary>显示名称</summary>
        string DisplayName { get; }

        /// <summary>分类</summary>
        string Category { get; }

        /// <summary>排序用基准分类名（特性原文，不随语言变化）</summary>
        string SortCategory { get; }

        /// <summary>排序用基准显示名（特性原文，不随语言变化）</summary>
        string SortDisplayName { get; }

        /// <summary>描述</summary>
        string Description { get; }

        /// <summary>是否只读</summary>
        bool IsReadOnly { get; }

        /// <summary>是否可浏览显示</summary>
        bool IsBrowsable { get; }

        /// <summary>属性类型</summary>
        Type PropertyType { get; }

        /// <summary>枚举值列表</summary>
        Array EnumValues { get; }

        /// <summary>是否可展开</summary>
        bool IsExpandable { get; }

        /// <summary>自定义编辑器类型</summary>
        CustomEditorType CustomEditor { get; }

        /// <summary>文件过滤器</summary>
        string FileFilter { get; }

        /// <summary>属性值</summary>
        object Value { get; set; }

        /// <summary>属性值的字符串表示</summary>
        string ValueString { get; set; }

        /// <summary>是否有初始值</summary>
        bool HasDefaultValue { get; }

        /// <summary>当前值是否等于初始值</summary>
        bool IsDefault { get; }

        /// <summary>属性值是否已被修改</summary>
        bool IsModified { get; }

        /// <summary>重置命令</summary>
        ICommand ResetCommand { get; }

        /// <summary>浏览文件命令</summary>
        ICommand BrowseFileCommand { get; }

        /// <summary>浏览目录命令</summary>
        ICommand BrowseDirectoryCommand { get; }

        /// <summary>编辑集合命令</summary>
        ICommand EditCollectionCommand { get; }

        /// <summary>按钮显示文本（[Button] 特性）</summary>
        string ButtonText { get; }

        /// <summary>按钮点击命令（[Button] 特性）</summary>
        ICommand ButtonClickCommand { get; }

        /// <summary>子属性列表</summary>
        System.Collections.Generic.List<PropertyItem> ChildProperties { get; }

        /// <summary>是否已展开</summary>
        bool IsExpanded { get; set; }

        /// <summary>值变更事件</summary>
        event EventHandler ValueChanged;

        /// <summary>按钮点击事件（[Button] 特性）</summary>
        event EventHandler<PropertyButtonClickEventArgs> ButtonClicked;

        /// <summary>属性变更事件</summary>
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>重置为默认值</summary>
        void ResetToDefault();

        /// <summary>刷新值通知</summary>
        void Refresh();
    }
}
