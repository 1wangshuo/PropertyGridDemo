using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 属性项工厂：从对象反射创建 PropertyItem 列表
    /// </summary>
    public static class PropertyItemFactory
    {
        /// <summary>
        /// 从对象创建所有可浏览的属性项
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="maxDepth">最大递归深度，防止循环引用导致栈溢出</param>
        /// <param name="currentDepth">当前递归深度</param>
        public static List<PropertyItem> CreateProperties(object obj, int maxDepth = 5, int currentDepth = 0)
        {
            if (obj == null) return new List<PropertyItem>();
            if (currentDepth >= maxDepth) return new List<PropertyItem>(); // 达到最大深度，停止递归

            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Select(p => new PropertyItem(p, obj, maxDepth, currentDepth))
                .Where(p => p.IsBrowsable)
                .OrderBy(p => p.Category, NaturalStringComparer.Instance)
                .ThenBy(p => p.DisplayName, NaturalStringComparer.Instance)
                .ThenBy(p => p.Name, NaturalStringComparer.Instance)
                .ToList();
        }

        /// <summary>
        /// 按分类分组；分类顺序与官方属性窗口（VS/WPF 设计器）一致：
        /// 常规分类按名称自然排序，「杂项 / Misc / Other」等收尾分类固定在最后。
        /// </summary>
        public static List<PropertyCategory> GroupByCategory(List<IPropertyItem> items)
        {
            return items
                .GroupBy(p => p.Category)
                .Select(g => new PropertyCategory(g.Key, g.ToList()))
                .OrderBy(c => IsMiscCategory(c.Name) ? 1 : 0)   // 杂项/其他 排最后
                .ThenBy(c => c.Name, NaturalStringComparer.Instance)
                .ToList();
        }

        /// <summary>
        /// 是否为应排在最后的"杂项/其他"分类（中英文常见命名）。
        /// 注意「常用/通用(Common)」不在此列 —— 官方属性窗口里 Common 通常排最前。
        /// </summary>
        public static bool IsMiscCategory(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var n = name.Trim();
            return n == "杂项" || n == "其他" || n == "其他属性"
                || n.Equals("Misc", StringComparison.OrdinalIgnoreCase)
                || n.Equals("Miscellaneous", StringComparison.OrdinalIgnoreCase)
                || n.Equals("Other", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// 属性分类组
    /// </summary>
    public class PropertyCategory : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PropertyCategory(string name, List<IPropertyItem> items)
        {
            Name = name;
            Items = items;
            _isExpanded = true;
        }

        public string Name { get; }
        public List<IPropertyItem> Items { get; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        /// <summary>
        /// 过滤后的属性项（搜索时使用）
        /// </summary>
        public List<IPropertyItem> FilteredItems
        {
            get => Items;
        }
    }
}
