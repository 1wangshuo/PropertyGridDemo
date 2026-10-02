using System;
using System.Collections.Generic;
using PropertyGridLib.Controls;

namespace PropertyGridDemo.Models
{
    /// <summary>
    /// 演示公式绑定树数据提供者
    /// </summary>
    public class DemoFormulaTreeProvider : IFormulaTreeProvider
    {
        public List<FormulaTreeNode> GetFormulaTree(PropertyItem propertyItem)
        {
            return new List<FormulaTreeNode>
            {
                new FormulaTreeNode
                {
                    Header = "全局变量",
                    Children =
                    {
                        new FormulaTreeNode { Header = "系统时间", Formula = "&{Global,SysTime,Value}", FormulaType = typeof(DateTime) },
                        new FormulaTreeNode { Header = "系统状态", Formula = "&{Global,SysStatus,Value}", FormulaType = typeof(string) }
                    }
                },
                new FormulaTreeNode
                {
                    Header = "流程1",
                    Children =
                    {
                        new FormulaTreeNode { Header = "任务1.结果", Formula = "&{Flow1,Task1,Result}", FormulaType = typeof(int) },
                        new FormulaTreeNode { Header = "任务1.状态", Formula = "&{Flow1,Task1,Status}", FormulaType = typeof(string) },
                        new FormulaTreeNode { Header = "任务2.输出", Formula = "&{Flow1,Task2,Output}", FormulaType = typeof(double) }
                    }
                },
                new FormulaTreeNode
                {
                    Header = "流程2",
                    Children =
                    {
                        new FormulaTreeNode { Header = "任务A.值", Formula = "&{Flow2,TaskA,Value}", FormulaType = typeof(int) },
                        new FormulaTreeNode { Header = "任务B.计数", Formula = "&{Flow2,TaskB,Count}", FormulaType = typeof(int) }
                    }
                },
                new FormulaTreeNode
                {
                    Header = "流程3",
                    Children =
                    {
                        new FormulaTreeNode { Header = "任务A.值", Formula = "&{Flow3,TaskA,Value}", FormulaType = typeof(int) },
                        new FormulaTreeNode { Header = "任务B.计数", Formula = "&{Flow3,TaskB,Count}", FormulaType = typeof(int) }
                    }
                },
                new FormulaTreeNode
                {
                    Header = "流程4",
                    Children =
                    {
                        new FormulaTreeNode { Header = "任务A.值", Formula = "&{Flow4,TaskA,Value}", FormulaType = typeof(int) },
                        new FormulaTreeNode { Header = "任务B.计数", Formula = "&{Flow4,TaskB,Count}", FormulaType = typeof(int) }
                    }
                },
                new FormulaTreeNode
                {
                    Header = "流程5",
                    Children =
                    {
                        new FormulaTreeNode { Header = "任务A.值", Formula = "&{Flow5,TaskA,Value}", FormulaType = typeof(int) },
                        new FormulaTreeNode { Header = "任务B.计数", Formula = "&{Flow5,TaskB,Count}", FormulaType = typeof(int) }
                    }
                },
                new FormulaTreeNode
                {
                    Header = "流程6",
                    Children =
                    {
                        new FormulaTreeNode { Header = "任务A.值", Formula = "&{Flow6,TaskA,Value}", FormulaType = typeof(int) },
                        new FormulaTreeNode { Header = "任务B.计数", Formula = "&{Flow6,TaskB,Count}", FormulaType = typeof(int) }
                    }
                }
            };
        }
    }
}
