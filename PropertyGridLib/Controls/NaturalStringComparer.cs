using System;
using System.Collections.Generic;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 自然排序比较器：模拟 Windows 资源管理器（StrCmpLogicalW）的自然排序行为。
    /// 连续数字段按数值比较（"2" 排在 "10" 之前），其余字符按不区分大小写比较，
    /// 因此 "1.名称"、"2.值"、"10.其他" 会按 1 → 2 → 10 的顺序排列，
    /// 字母（a、b、c）和罗马数字（I、II、III、IV、V、VI）也能按预期排序。
    /// </summary>
    public sealed class NaturalStringComparer : IComparer<string>
    {
        /// <summary>共享单例实例。</summary>
        public static readonly NaturalStringComparer Instance = new NaturalStringComparer();

        private NaturalStringComparer()
        {
        }

        public int Compare(string x, string y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int ix = 0, iy = 0;
            int lx = x.Length, ly = y.Length;

            while (ix < lx && iy < ly)
            {
                char cx = x[ix];
                char cy = y[iy];

                if (char.IsDigit(cx) && char.IsDigit(cy))
                {
                    // 定位两边的连续数字段
                    int ex = ix, ey = iy;
                    while (ex < lx && char.IsDigit(x[ex])) ex++;
                    while (ey < ly && char.IsDigit(y[ey])) ey++;

                    // 跳过前导零后按有效数字长度比较
                    int sx = ix, sy = iy;
                    while (sx < ex && x[sx] == '0') sx++;
                    while (sy < ey && y[sy] == '0') sy++;

                    int nx = ex - sx;
                    int ny = ey - sy;
                    if (nx != ny) return nx < ny ? -1 : 1;

                    // 有效数字长度相同时逐位比较
                    while (sx < ex)
                    {
                        if (x[sx] != y[sy]) return x[sx] < y[sy] ? -1 : 1;
                        sx++;
                        sy++;
                    }

                    ix = ex;
                    iy = ey;
                }
                else
                {
                    // 非数字字符按不区分大小写比较
                    int cmp = char.ToUpperInvariant(cx).CompareTo(char.ToUpperInvariant(cy));
                    if (cmp != 0) return cmp;
                    ix++;
                    iy++;
                }
            }

            // 其中一方已到末尾：剩余长度短的排在前面
            int remX = lx - ix;
            int remY = ly - iy;
            if (remX == remY) return 0;
            return remX < remY ? -1 : 1;
        }
    }
}
