using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyGridLib.Controls;
using PropertyGridDemo.Models;

namespace PropertyGridDemo
{
    public partial class CollectionEditorDemoWindow : HandyControl.Controls.Window, INotifyPropertyChanged
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

        // ===== SDK 独立控件演示：FilePathControl / FormulaSwitchControl =====

        private string _sdkFilePath = "D:\\demo\\cam.log";
        public string SdkFilePath
        {
            get => _sdkFilePath;
            set { _sdkFilePath = value; OnPropertyChanged(); }
        }

        private string _sdkDirectory = "D:\\demo";
        public string SdkDirectory
        {
            get => _sdkDirectory;
            set { _sdkDirectory = value; OnPropertyChanged(); }
        }

        private string _sdkSwitchText = "直接输入的值";
        public string SdkSwitchText
        {
            get => _sdkSwitchText;
            set { _sdkSwitchText = value; OnPropertyChanged(); }
        }

        private string _sdkSwitchFormula = "&{Sensors,Temperature,Value}";
        public string SdkSwitchFormula
        {
            get => _sdkSwitchFormula;
            set { _sdkSwitchFormula = value; OnPropertyChanged(); }
        }

        // ===== 独立控件演示：EventEditorControl / CollectionEditorControl / DictionaryEditorControl =====

        private RangeValue _sdkRange = new RangeValue(120, 480);
        /// <summary>范围滑块独立用法：区间值（与 RangeSliderControl.Value 双向）</summary>
        public RangeValue SdkRange
        {
            get => _sdkRange;
            set
            {
                if (_sdkRange != null && _sdkRange.Equals(value)) return;
                _sdkRange = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SdkRangeText));
            }
        }

        /// <summary>宿主直接读取 .Min / .Max 即拿到两端值</summary>
        public string SdkRangeText
            => _sdkRange == null ? "（未设置）" : $"Min = {_sdkRange.Min}    Max = {_sdkRange.Max}";

        private string _sdkHandlerName = "";
        /// <summary>事件编辑器独立用法：处理方法名（与 EventEditorControl.HandlerName 双向）</summary>
        public string SdkHandlerName
        {
            get => _sdkHandlerName;
            set
            {
                if (_sdkHandlerName == value) return;
                _sdkHandlerName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>集合编辑器独立用法：宿主自己的集合，控件直接编辑它</summary>
        public System.Collections.ObjectModel.ObservableCollection<string> DemoCollection { get; }
            = new System.Collections.ObjectModel.ObservableCollection<string> { "张三", "李四", "王五" };

        /// <summary>字典编辑器独立用法：宿主自己的字典</summary>
        public Dictionary<string, int> DemoDictionary { get; }
            = new Dictionary<string, int> { { "波特率", 115200 }, { "数据位", 8 }, { "重试次数", 3 } };

        /// <summary>集合当前内容摘要（演示控件直接改宿主集合）</summary>
        public string DemoCollectionSummary
        {
            get
            {
                var items = DemoCollection;
                return items.Count == 0 ? "（空）" : $"[{items.Count} 项] " + string.Join("、", items);
            }
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
