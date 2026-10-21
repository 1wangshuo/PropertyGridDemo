using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 事件项：PropertyGrid 事件页 / EventPicker 中的一行（PropertyGridLib 1.4.0 新增）。
    /// 包装一个 CLR 事件（名称、委托类型、当前处理方法名）；
    /// 设计器宿主可继承并重写 <see cref="HandlerName"/> 把读写路由到自己的模型（如 WpfDesign 的 DesignItemProperty）。
    /// </summary>
    public class EventItem : INotifyPropertyChanged
    {
        private string _handlerName;
        private string _displayName;

        /// <summary>创建事件项</summary>
        /// <param name="name">事件名</param>
        /// <param name="eventType">事件委托类型（如 RoutedEventHandler）</param>
        /// <param name="handlerName">已绑定的处理方法名（可空）</param>
        /// <param name="category">分类（可空，默认“事件”）</param>
        /// <param name="description">描述（可空）</param>
        /// <param name="displayName">显示名（可空，默认与 name 相同）</param>
        public EventItem(string name, Type eventType, string handlerName = null,
            string category = null, string description = null, string displayName = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            EventType = eventType;
            _handlerName = handlerName;
            _displayName = displayName;
            Category = category ?? LocalizationManager.GetString("EventsCategory");
            Description = description ?? string.Empty;
            EditCommand = new RelayCommand(_ => RaiseEditRequested());
        }

        /// <summary>从 EventInfo 反射构建（读取 [DisplayName]/[Category]/[Description] 特性，无 [DisplayName] 时查宿主注册的属性/事件名翻译表）</summary>
        public static EventItem FromEventInfo(EventInfo info)
        {
            if (info == null) return null;
            string displayName = null, category = null, description = null;
            try
            {
                foreach (var a in info.GetCustomAttributes(true))
                {
                    if (a is DisplayNameAttribute d) displayName = d.DisplayName;
                    else if (a is CategoryAttribute c) category = c.Category;
                    else if (a is DescriptionAttribute de) description = de.Description;
                }
            }
            catch { }

            if (string.IsNullOrEmpty(displayName))
                displayName = LocalizationManager.TryGetPropertyName(info.Name);

            return new EventItem(info.Name, info.EventHandlerType, null, category, description, displayName);
        }

        private bool _isGroupFirst;
        private bool _isGroupLast;

        /// <summary>
        /// 是否为其分类的第一项（分类"卡片"样式的起手：决定这一行要不要画上边框/上圆角）。
        /// 由 <c>EventPicker</c> 在压平行集合时写入。
        /// </summary>
        public bool IsGroupFirst
        {
            get => _isGroupFirst;
            internal set
            {
                if (_isGroupFirst == value) return;
                _isGroupFirst = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGroupFirst)));
            }
        }

        /// <summary>是否为其分类的最后一项（负责补下边框与下圆角，把一组事件收成卡片）。</summary>
        public bool IsGroupLast
        {
            get => _isGroupLast;
            internal set
            {
                if (_isGroupLast == value) return;
                _isGroupLast = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGroupLast)));
            }
        }

        /// <summary>事件名（代码中的名称）</summary>
        public string Name { get; }

        /// <summary>事件委托类型</summary>
        public Type EventType { get; }

        /// <summary>分类（分组显示）</summary>
        public string Category { get; }

        /// <summary>描述（提示）</summary>
        public string Description { get; }

        /// <summary>是否带描述。底部描述栏只在「确有详细介绍」时出现（占位不占屏）。</summary>
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        /// <summary>显示名（[DisplayName] 特性优先，否则与 Name 相同）</summary>
        public string DisplayName => _displayName ?? Name;

        /// <summary>类型提示，如 “(RoutedEventHandler)”</summary>
        public string TypeHint => "(" + (EventType?.Name ?? "event") + ")";

        /// <summary>是否只读（禁止修改处理方法名）</summary>
        public bool IsReadOnly { get; set; }

        /// <summary>当前事件处理方法名（双向可编辑；宿主可重写以写入设计模型）</summary>
        public virtual string HandlerName
        {
            get => _handlerName;
            set
            {
                if (string.Equals(_handlerName, value, StringComparison.Ordinal)) return;
                _handlerName = value;
                OnPropertyChanged(nameof(HandlerName));
                OnPropertyChanged(nameof(HasHandler));
            }
        }

        /// <summary>是否已绑定处理方法</summary>
        public bool HasHandler => !string.IsNullOrWhiteSpace(HandlerName);

        /// <summary>“编辑代码”命令：触发 <see cref="EditRequested"/>（无订阅者时执行默认行为）</summary>
        public ICommand EditCommand { get; }

        /// <summary>请求编辑该事件的处理代码（双击行或点击 ⚡ 按钮时触发）</summary>
        public event EventHandler EditRequested;

        private void RaiseEditRequested()
        {
            if (EditRequested != null)
            {
                EditRequested.Invoke(this, EventArgs.Empty);
            }
            else if (!HasHandler && !IsReadOnly)
            {
                // 默认行为：没有宿主接管时，填入建议方法名 On<事件名>
                HandlerName = "On" + Name;
            }
        }

        /// <summary>供子类触发属性变更通知</summary>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>属性变更通知（供模板绑定刷新）</summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>事件项工厂：从普通 CLR 对象反射生成事件列表</summary>
    public static class EventItemFactory
    {
        /// <summary>枚举 obj 的所有公共实例事件（含 [DisplayName]/[Category]/[Description]），按分类+名称排序</summary>
        public static List<EventItem> CreateEvents(object obj)
        {
            var result = new List<EventItem>();
            if (obj == null) return result;

            foreach (var ev in obj.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance))
            {
                var item = EventItem.FromEventInfo(ev);
                if (item != null) result.Add(item);
            }

            return result
                .OrderBy(e => PropertyItemFactory.IsMiscCategory(e.Category) ? 1 : 0)  // 杂项/其他 分类排最后
                .ThenBy(e => e.Category, NaturalStringComparer.Instance)
                .ThenBy(e => e.DisplayName, NaturalStringComparer.Instance)
                .ToList();
        }
    }

    /// <summary>事件编辑请求事件参数</summary>
    public class EventItemEventArgs : EventArgs
    {
        /// <summary>请求编辑的事件项</summary>
        public EventItem Item { get; }

        public EventItemEventArgs(EventItem item)
        {
            Item = item;
        }
    }
}
