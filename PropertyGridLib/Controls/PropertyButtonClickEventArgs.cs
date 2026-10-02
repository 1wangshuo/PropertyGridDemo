using System;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 当标记了 <c>[Button]</c> 特性的属性按钮被点击时携带的事件参数。
    /// 同时用于 <c>PropertyItem.ButtonClicked</c> 与 <c>PropertyGrid.ButtonClicked</c> 事件。
    /// </summary>
    public class PropertyButtonClickEventArgs : EventArgs
    {
        /// <summary>被点击的属性项</summary>
        public IPropertyItem PropertyItem { get; }

        /// <summary>属性所属的宿主对象（即 PropertyGrid.SelectedObject 或其嵌套子对象）</summary>
        public object Owner { get; }

        /// <summary>属性名称（代码中的名称）</summary>
        public string PropertyName { get; }

        public PropertyButtonClickEventArgs(IPropertyItem propertyItem, object owner, string propertyName)
        {
            PropertyItem = propertyItem;
            Owner = owner;
            PropertyName = propertyName;
        }
    }
}
