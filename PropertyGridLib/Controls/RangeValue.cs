using System;
using System.Globalization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 范围值（区间）：<see cref="Min"/> / <see cref="Max"/> 都是 double。
    /// 属性声明为该类型时，PropertyGrid 自动使用范围滑块编辑器（无需特性）；
    /// 需要限定边界与步长时配合 <see cref="Attributes.RangeSliderAttribute"/> 使用，例如
    /// <c>[RangeSlider(0, 1920, 8)] public RangeValue DetectRange { get; set; }</c>。
    /// 宿主直接读 <c>DetectRange.Min</c> / <c>DetectRange.Max</c> 即可拿到两端值。
    /// </summary>
    public class RangeValue : IEquatable<RangeValue>
    {
        public RangeValue()
        {
        }

        public RangeValue(double min, double max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>区间起点（较小的一端由滑块保证，数据本身不做强制排序）</summary>
        public double Min { get; set; }

        /// <summary>区间终点</summary>
        public double Max { get; set; }

        /// <summary>复制一份（编辑器写回时用新实例，避免就地修改宿主共享对象）</summary>
        public RangeValue Clone() => new RangeValue(Min, Max);

        public bool Equals(RangeValue other)
            => other != null && other.Min == Min && other.Max == Max;

        public override bool Equals(object obj) => Equals(obj as RangeValue);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
            }
        }

        /// <summary>显示文本：2 ~ 8（G15 去掉二进制舍入噪声）</summary>
        public override string ToString()
            => $"{Format(Min)} ~ {Format(Max)}";

        private static string Format(double value)
            => value.ToString("G15", CultureInfo.CurrentCulture);
    }
}
