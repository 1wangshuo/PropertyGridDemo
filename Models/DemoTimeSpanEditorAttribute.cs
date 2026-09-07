using System;

namespace PropertyGridDemo.Models
{
    /// <summary>
    /// Demo 自定义编辑器特性：数值 + 单位下拉
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DemoTimeSpanEditorAttribute : Attribute
    {
        /// <summary>可用单位列表，逗号分隔。例如 "ms,s,min,h"</summary>
        public string Units { get; set; }

        /// <summary>默认选中的单位</summary>
        public string DefaultUnit { get; set; }

        public DemoTimeSpanEditorAttribute(string units, string defaultUnit = "")
        {
            Units = units;
            DefaultUnit = defaultUnit;
        }
    }
}
