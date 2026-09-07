using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyGridLib.Controls;
using PropertyGridDemo.Models;

namespace PropertyGridDemo
{
    public partial class CollectionEditorDemoWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<FormulaTreeNode> DirectTreeNodes { get; }

        private string _formula1 = "&{Global,SysTime,Value}";
        private string _formula2 = "&{Sensors,Temperature,Value}";

        public string Formula1
        {
            get => _formula1;
            set { _formula1 = value; OnPropertyChanged(); UpdateFormulaValues(); }
        }

        public string Formula2
        {
            get => _formula2;
            set { _formula2 = value; OnPropertyChanged(); UpdateFormulaValues(); }
        }

        public CollectionEditorDemoWindow()
        {
            DirectTreeNodes = new List<FormulaTreeNode>
            {
                new FormulaTreeNode
                {
                    Header = "直接绑定节点",
                    Children =
                    {
                        new FormulaTreeNode { Header = "温度传感器", Formula = "&{Sensors,Temperature,Value}" },
                        new FormulaTreeNode { Header = "湿度传感器", Formula = "&{Sensors,Humidity,Value}" },
                        new FormulaTreeNode { Header = "压力传感器", Formula = "&{Sensors,Pressure,Value}" }
                    }
                },
                new FormulaTreeNode
                {
                    Header = "计算结果",
                    Children =
                    {
                        new FormulaTreeNode { Header = "平均值", Formula = "&{Calc,Average,Result}" },
                        new FormulaTreeNode { Header = "最大值", Formula = "&{Calc,Max,Result}" }
                    }
                }
            };

            InitializeComponent();
            DataContext = this;

            ShowStringList_Click(null, null);
            ShowStringIntDict_Click(null, null);
            UpdateFormulaValues();
        }

        private void ShowStringList_Click(object sender, RoutedEventArgs e)
        {
            collectionPropertyGrid.SelectedObject = new List<string>
            {
                "苹果", "香蕉", "橙子", "葡萄", "西瓜"
            };
        }

        private void ShowIntList_Click(object sender, RoutedEventArgs e)
        {
            collectionPropertyGrid.SelectedObject = new List<int>
            {
                100, 200, 300, 400, 500
            };
        }

        private void ShowObjectList_Click(object sender, RoutedEventArgs e)
        {
            collectionPropertyGrid.SelectedObject = new List<SampleObject>
            {
                new SampleObject { CameraName = "对象A", RtspPort = 100 },
                new SampleObject { CameraName = "对象B", RtspPort = 300 },
                new SampleObject { CameraName = "对象C", RtspPort = 500 }
            };
        }

        // ==================== 字典编辑器 ====================

        private void ShowStringIntDict_Click(object sender, RoutedEventArgs e)
        {
            dictionaryPropertyGrid.SelectedObject = new Dictionary<string, int>
            {
                { "苹果", 5 }, { "香蕉", 3 }, { "橙子", 8 }, { "葡萄", 12 }
            };
        }

        private void ShowComplexValueDict_Click(object sender, RoutedEventArgs e)
        {
            dictionaryPropertyGrid.SelectedObject = new Dictionary<string, SampleObject2>
            {
                { "节点A", new SampleObject2() },
                { "节点B", new SampleObject2() }
            };
        }

        private void ShowComplexKeyDict_Click(object sender, RoutedEventArgs e)
        {
            dictionaryPropertyGrid.SelectedObject = new Dictionary<ServerEndpoint, string>
            {
                { new ServerEndpoint { Host = "192.168.1.10", Port = 8080 }, "主服务器" },
                { new ServerEndpoint { Host = "192.168.1.11", Port = 8081 }, "备服务器" }
            };
        }

        private void ShowBothComplexDict_Click(object sender, RoutedEventArgs e)
        {
            dictionaryPropertyGrid.SelectedObject = new Dictionary<ServerEndpoint, SampleObject2>
            {
                { new ServerEndpoint { Host = "192.168.1.10", Port = 8080 }, new SampleObject2() },
                { new ServerEndpoint { Host = "192.168.1.11", Port = 8081 }, new SampleObject2() }
            };
        }

        private void UpdateFormulaValues()
        {
            if (formulaValuesText == null) return;
            formulaValuesText.Text =
                $"方式一 (Provider): {Formula1}\n" +
                $"方式二 (TreeItemsSource): {Formula2}";
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// 字典复杂键演示类型
    /// </summary>
    public class ServerEndpoint
    {
        [Category("端点")]
        [DisplayName("主机地址")]
        public string Host { get; set; } = string.Empty;

        [Category("端点")]
        [DisplayName("端口")]
        public int Port { get; set; }

        public override bool Equals(object obj) =>
            obj is ServerEndpoint o && Host == o.Host && Port == o.Port;

        public override int GetHashCode() =>
            ((Host?.GetHashCode() ?? 0) * 397) ^ Port.GetHashCode();

        public override string ToString() => $"{Host}:{Port}";
    }
}
