using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 标记属性为“命令按钮”编辑器。
    /// 被标记的属性在 PropertyGrid 中不再显示值编辑器，而是渲染为一个按钮；
    /// 点击后会：1) 通过反射调用宿主对象上 <see cref="Action"/> 指定的方法（若指定）；
    /// 2) 触发 <c>PropertyItem.ButtonClicked</c> 与 <c>PropertyGrid.ButtonClicked</c> 事件，供宿主统一响应。
    /// 用法：
    ///   [Button]                                  // 文本回退到 DisplayName，仅触发事件
    ///   [Button("测试连接")]                       // 指定按钮文本，仅触发事件
    ///   [Button("测试连接", nameof(TestConnection))] // 指定文本 + 点击时调用的宿主方法
    /// 由于按钮无“值”语义，建议将属性声明为 object/string 占位类型（值可为 null）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ButtonAttribute : Attribute
    {
        /// <summary>
        /// 按钮显示文本。为空时回退到属性的 DisplayName（随多语言切换而变化）。
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 点击按钮时在宿主对象上调用的方法名（可选）。
        /// 通过反射调用：支持无参方法，或单个参数（参数类型为 PropertyItem 或宿主对象本身）的方法。
        /// 未指定时，点击仅触发 ButtonClicked 事件，由宿主自行处理。
        /// </summary>
        public string Action { get; set; }

        /// <param name="text">按钮显示文本；为空时回退到 DisplayName。</param>
        /// <param name="action">点击时调用的宿主方法名；为空时仅触发事件。</param>
        public ButtonAttribute(string text = null, string action = null)
        {
            Text = text;
            Action = action;
        }
    }
}
