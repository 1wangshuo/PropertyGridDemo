using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 数字滑块特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NumberSliderAttribute : Attribute
    {
        /// <summary>
        /// 最小值
        /// </summary>
        public double Minimum { get; set; } = 0;

        /// <summary>
        /// 最大值
        /// </summary>
        public double Maximum { get; set; } = 100;

        /// <summary>
        /// 步长值
        /// </summary>
        public double Step { get; set; } = 1;

        /// <summary>
        /// 是否在滑块旁显示数字输入框（hc:NumericUpDown）。
        /// true 时显示可编辑的数字框（min/max/step 与滑块共用同一套参数，双向同步），
        /// 取代只读的数值标签；false（默认）时显示只读数值标签。
        /// </summary>
        public bool ShowNumberBox { get; set; } = false;

        public NumberSliderAttribute()
        {
        }

        public NumberSliderAttribute(double minimum, double maximum, double step = 1)
        {
            Minimum = minimum;
            Maximum = maximum;
            Step = step;
        }
    }
}
