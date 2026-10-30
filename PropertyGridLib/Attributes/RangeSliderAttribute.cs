using System;

namespace PropertyGridLib.Attributes
{
    /// <summary>
    /// 范围滑块特性：作用于 <see cref="Controls.RangeValue"/> 类型的属性，
    /// 指定范围滑块的最小值、最大值与刻度步长。
    /// 不写该特性时按默认 0 ~ 100、步长 1 显示（类型本身即已自动映射为范围滑块）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RangeSliderAttribute : Attribute
    {
        /// <summary>最小值</summary>
        public double Minimum { get; set; } = 0;

        /// <summary>最大值</summary>
        public double Maximum { get; set; } = 100;

        /// <summary>每次增加的刻度（步长）</summary>
        public double Step { get; set; } = 1;

        public RangeSliderAttribute()
        {
        }

        public RangeSliderAttribute(double minimum, double maximum, double step = 1)
        {
            Minimum = minimum;
            Maximum = maximum;
            Step = step;
        }
    }
}
