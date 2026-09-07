using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using PropertyGridLib.Attributes;
using PropertyGridLib;
using PropertyGridLib.Controls;
using PropertyGridLib.Localization;

namespace PropertyGridDemo.Models
{
    /// <summary>
    /// 演示对象：智能监控摄像头配置
    /// </summary>
    [Serializable]
    public class SampleObject : INotifyPropertyChanged, IPropertyLocalization
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // ============================================================
        // 1. 基本信息
        // ============================================================

        [Category("I. 基本信息")]
        [DisplayName("摄像头名称")]
        [Description("摄像头的显示名称")]
        [DefaultValue("前门")]
        public string CameraName { get; set; } = "前门";

        [Category("I. 基本信息")]
        [DisplayName("启用状态")]
        [Description("是否启用此摄像头")]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        [Category("I. 基本信息")]
        [DisplayName("摄像头类型")]
        [Description("摄像头的类型")]
        [DefaultValue(CameraType.Bullet)]
        public CameraType CameraType { get; set; } = CameraType.Bullet;

        [Category("I. 基本信息")]
        [DisplayName("安装日期")]
        [Description("摄像头的安装日期")]
        public DateTime InstallDate { get; set; } = new DateTime(2024, 3, 15);

        [Category("I. 基本信息")]
        [DisplayName("保修到期")]
        [Description("保修到期日期，可空")]
        public DateTime? WarrantyExpiry { get; set; } = new DateTime(2027, 3, 15);

        // ============================================================
        // II. 外观与显示
        // ============================================================

        [Category("II. 外观与显示")]
        [DisplayName("主体颜色")]
        [Description("摄像头外壳的主色调（框架内置 Color 编辑器）")]
        public Color BodyColor { get; set; } = Colors.DimGray;

        [Category("II. 外观与显示")]
        [DisplayName("指示灯颜色")]
        [Description("摄像头运行时指示灯的颜色（框架内置 Color 编辑器）")]
        public SolidColorBrush LedColor { get; set; } = new SolidColorBrush(Colors.LimeGreen);

        [Category("II. 外观与显示")]
        [DisplayName("夜视补光颜色")]
        [Description("夜视模式下的补光颜色，可空（框架内置 Color 编辑器）")]
        public Color? NightVisionColor { get; set; } = Colors.OrangeRed;

        [Category("II. 外观与显示")]
        [DisplayName("视频亮度")]
        [Description("视频画面亮度（0-100）（框架内置 NumberSlider 编辑器）")]
        [DefaultValue(50)]
        [NumberSlider(0, 100, 5)]
        public int Brightness { get; set; } = 50;

        [Category("II. 外观与显示")]
        [DisplayName("视频对比度")]
        [Description("视频画面对比度（0.0-2.0）（框架内置 NumberSlider + ShowNumberBox）")]
        [DefaultValue(1.0)]
        [NumberSlider(0.0, 2.0, 0.1, ShowNumberBox = true)]
        public double Contrast { get; set; } = 1.0;

        // ============================================================
        // III. 网络配置
        // ============================================================

        [Category("III. 网络配置")]
        [DisplayName("IP地址")]
        [Description("摄像头的IP地址")]
        [DefaultValue("192.168.1.108")]
        public string IpAddress { get; set; } = "192.168.1.108";

        [Category("III. 网络配置")]
        [DisplayName("RTSP端口")]
        [Description("RTSP流媒体协议端口")]
        [DefaultValue(554)]
        public int RtspPort { get; set; } = 554;

        [Category("III. 网络配置")]
        [DisplayName("用户名")]
        [Description("登录摄像头的用户名")]
        [DefaultValue("admin")]
        public string Username { get; set; } = "admin";

        [Category("III. 网络配置")]
        [DisplayName("启用HTTPS")]
        [Description("是否使用HTTPS加密连接")]
        public bool UseHttps { get; set; } = false;

        [Category("III. 网络配置")]
        [DisplayName("传输协议")]
        [Description("视频流传输协议")]
        [DefaultValue(TransportProtocol.Rtsp)]
        public TransportProtocol Protocol { get; set; } = TransportProtocol.Rtsp;

        // ============================================================
        // IV. 存储路径
        // ============================================================

        [Category("IV. 存储路径")]
        [DisplayName("录像存储目录")]
        [Description("录像文件的保存位置（框架内置 DirectoryPath 编辑器）")]
        [DirectoryPath]
        public string RecordDirectory { get; set; } = "";

        [Category("IV. 存储路径")]
        [DisplayName("截图存储目录")]
        [Description("抓拍截图的保存位置（框架内置 DirectoryPath 编辑器）")]
        [DirectoryPath]
        public string SnapshotDirectory { get; set; } = "";

        [Category("IV. 存储路径")]
        [DisplayName("配置导入文件")]
        [Description("导入摄像头配置文件（.json / .xml）（框架内置 FilePath 编辑器）")]
        [FilePath("*.json", "*.xml")]
        public string ConfigImportFile { get; set; } = "";

        [Category("IV. 存储路径")]
        [DisplayName("日志文件路径")]
        [Description("摄像头运行日志（.log）（框架内置 FilePath 编辑器）")]
        [FilePath("*.log")]
        public string LogFilePath { get; set; } = "";

        // ============================================================
        // V. 高级配置
        // ============================================================

        [Category("V. 高级配置")]
        [DisplayName("选择NVR通道")]
        [Description("下拉选择NVR通道（框架内置 TypeConverter 下拉）")]
        [TypeConverter(typeof(ComPortListConverter))]
        public string NvrChannel { get; set; } = "通道1";

        [Category("V. 高级配置")]
        [DisplayName("启用的智能分析")]
        [Description("多选启用的AI分析功能（框架内置 MultiSelect 多选）")]
        [MultiSelect(typeof(DemoMultiSelectProvider))]
        public List<string> EnabledAnalyses { get; set; } = new List<string> { "人员检测", "车辆检测" };

        [Category("V. 高级配置")]
        [DisplayName("录像切片时长")]
        [Description("录像文件切片时长（分钟）")]
        [DefaultValue(30)]
        public int SliceMinutes { get; set; } = 30;

        [Category("V. 高级配置")]
        [DisplayName("是否循环录像")]
        [Description("磁盘满时是否覆盖最早的录像")]
        public bool LoopRecording { get; set; } = true;

        // ============================================================
        // VI. 检测区域
        // ============================================================

        [Category("VI. 检测区域")]
        [DisplayName("主检测区域")]
        [Description("可展开编辑主检测区域的详细参数（框架内置 Expandable，子属性含 NumberSlider）")]
        public DetectionZone MainZone { get; set; } = new DetectionZone { Name = "主区域", Sensitivity = 7 };

        [Category("VI. 检测区域")]
        [DisplayName("禁区配置")]
        [Description("可展开编辑禁区的详细参数（框架内置 Expandable）")]
        public DetectionZone RestrictedZone { get; set; } = new DetectionZone { Name = "禁区", Sensitivity = 10 };

        // ============================================================
        // VII. 集合编辑器
        // ============================================================

        [Category("VII. 集合编辑器")]
        [DisplayName("值班人员列表")]
        [Description("简单字符串集合（List<string>）（框架内置 Collection 编辑器）")]
        [CollectionEditor]
        public List<string> DutyPersonnel { get; set; } = new List<string> { "张三", "李四", "王五" };

        [Category("VII. 集合编辑器")]
        [DisplayName("预置巡航点位")]
        [Description("复杂对象集合（List<CruisePoint>），内嵌 PropertyGrid 编辑（框架内置 Collection 编辑器）")]
        [CollectionEditor]
        public List<CruisePoint> CruisePoints { get; set; } = new List<CruisePoint>
        {
            new CruisePoint { PresetName = "大门", Horizontal = 180, Vertical = 90, StaySeconds = 5 },
            new CruisePoint { PresetName = "停车场", Horizontal = 45, Vertical = 60, StaySeconds = 10 }
        };

        // ============================================================
        // VIII. 字典编辑器
        // ============================================================

        [Category("VIII. 字典编辑器")]
        [DisplayName("区域灵敏度")]
        [Description("Dictionary<string, int> —— 简单键值（框架内置 Dictionary 编辑器）")]
        public Dictionary<string, int> ZoneSensitivity { get; set; } = new Dictionary<string, int>
        {
            { "大门", 8 }, { "停车场", 5 }, { "走廊", 6 }
        };

        [Category("VIII. 字典编辑器")]
        [DisplayName("预置位坐标映射")]
        [Description("Dictionary<string, CruisePoint> —— 值为复杂对象，内嵌 PropertyGrid 就地编辑（框架内置 Dictionary 编辑器）")]
        public Dictionary<string, CruisePoint> PresetCoordinates { get; set; } = new Dictionary<string, CruisePoint>
        {
            { "大门", new CruisePoint { PresetName = "大门", Horizontal = 180, Vertical = 90, StaySeconds = 5 } },
            { "停车场", new CruisePoint { PresetName = "停车场", Horizontal = 45, Vertical = 60, StaySeconds = 10 } }
        };

        [Category("VIII. 字典编辑器")]
        [DisplayName("嵌套区域配置")]
        [Description("Dictionary<string, Dictionary<string, int>> —— 值为嵌套字典，显示「N 项 ...」按钮弹窗（框架内置 Dictionary 编辑器，避免 UI 拉长）")]
        public Dictionary<string, Dictionary<string, int>> NestedZoneConfig { get; set; } = new Dictionary<string, Dictionary<string, int>>
        {
            {
                "白天模式",
                new Dictionary<string, int> { { "人员检测", 6 }, { "车辆检测", 5 } }
            },
            {
                "夜间模式",
                new Dictionary<string, int> { { "人员检测", 8 }, { "车辆检测", 7 } }
            }
        };

        // ============================================================
        // IX. 公式绑定
        // ============================================================

        [Category("IX. 公式绑定")]
        [DisplayName("摄像头名称绑定")]
        [Description("FormulaBound<string> — 点击链接图标选择绑定源（框架内置 Formula 编辑器）")]
        [FormulaEditor(typeof(DemoFormulaTreeProvider))]
        public FormulaBound<string> NameFormula { get; set; } = new FormulaBound<string> { Value = "前门" };

        [Category("IX. 公式绑定")]
        [DisplayName("录像时长绑定")]
        [Description("FormulaBound<int> — int 类型公式绑定（框架内置 Formula 编辑器）")]
        [FormulaEditor(typeof(DemoFormulaTreeProvider))]
        public FormulaBound<int> DurationFormula { get; set; } = new FormulaBound<int> { Value = 30 };

        // ============================================================
        // X. 操作按钮
        // ============================================================

        [Category("X. 操作按钮")]
        [DisplayName("抓拍一张")]
        [Description("点击调用模型内的 TakeSnapshot 方法（框架内置 [Button] + Action 反射调用）")]
        [Button("抓拍", nameof(TakeSnapshot))]
        public object? SnapshotButton { get; set; }

        [Category("X. 操作按钮")]
        [DisplayName("重启摄像头")]
        [Description("点击调用模型内的 Reboot 方法（框架内置 [Button] 副作用演示）")]
        [Button("重启", nameof(Reboot))]
        public object? RebootButton { get; set; }

        [Category("X. 操作按钮")]
        [DisplayName("导出配置")]
        [Description("未指定 Action，点击仅触发 PropertyGrid.ButtonClicked 事件（框架内置 [Button]）")]
        [Button]
        public object? ExportButton { get; set; }

        [Category("X. 操作按钮")]
        [DisplayName("抓拍次数")]
        [Description("抓拍按钮被点击的累计次数（只读展示）")]
        [ReadOnly(true)]
        public int SnapshotCount => _snapshotCount;

        // ===== [Button] 调用的字段和方法 =====
        private int _snapshotCount;
        public void TakeSnapshot() { _snapshotCount++; OnPropertyChanged(nameof(SnapshotCount)); }
        public void Reboot() { OnPropertyChanged(nameof(SnapshotCount)); }
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ============================================================
        // XI. 宿主扩展编辑器
        // ============================================================

        [Category("XI. 宿主扩展编辑器 ★")]
        [DisplayName("录像文件大小")]
        [Description("★宿主扩展：数值 + 单位下拉（GB/MB/KB）—— Demo 侧自写 Attribute + DataTemplate，RegisterEditor 注册后生效")]
        [DemoTimeSpanEditor("GB,MB,KB", "MB")]
        public double FileSize { get; set; } = 256;

        [Category("XI. 宿主扩展编辑器 ★")]
        [DisplayName("焦距")]
        [Description("★宿主扩展：数值 + 单位下拉（mm/倍）—— 框架对此 Attribute 一无所知")]
        [DemoTimeSpanEditor("mm,倍", "mm")]
        public double FocalLength { get; set; } = 3.6;

        [Category("XI. 宿主扩展编辑器 ★")]
        [DisplayName("网络延迟")]
        [Description("★宿主扩展：数值 + 单位下拉（ms/s）—— 证明扩展机制可完全替代框架内置编辑器")]
        [DemoTimeSpanEditor("ms,s", "ms")]
        [DefaultValue(150)]
        public int NetworkLatency { get; set; } = 150;

        // ============================================================
        // XII. 只读与隐藏
        // ============================================================

        [Category("XII. 只读与隐藏")]
        [DisplayName("固件版本")]
        [Description("只读属性（框架内置 [ReadOnly]）")]
        [ReadOnly(true)]
        public string FirmwareVersion { get; } = "v2.1.5-release";

        [Category("XII. 只读与隐藏")]
        [DisplayName("硬件序列号")]
        [Description("只读属性（框架内置 [ReadOnly]）")]
        [ReadOnly(true)]
        public string HardwareSn { get; } = "SN-20240315-00108";

        [Browsable(false)]
        public string InternalGuid { get; set; } = "550e8400-e29b-41d4-a716-446655440000";

        [Browsable(false)]
        public int InternalFrameCount { get; set; } = 0;

        // ============================================================
        // XIII. 自然排序
        // ============================================================

        [Category("XIII. 排序演示")]
        [DisplayName("3.参数三")]
        public string SortParam3 { get; set; } = "三";

        [Category("XIII. 排序演示")]
        [DisplayName("1.参数一")]
        public string SortParam1 { get; set; } = "一";

        [Category("XIII. 排序演示")]
        [DisplayName("10.参数十")]
        public string SortParam10 { get; set; } = "十";

        [Category("XIII. 排序演示")]
        [DisplayName("2.参数二")]
        public string SortParam2 { get; set; } = "二";

        [Category("XIII. 排序演示")]
        [DisplayName("I.罗马一")]
        public string SortParamRoman1 { get; set; } = "I";

        [Category("XIII. 排序演示")]
        [DisplayName("II.罗马二")]
        public string SortParamRoman2 { get; set; } = "II";

        [Category("XIII. 排序演示")]
        [DisplayName("III.罗马三")]
        public string SortParamRoman3 { get; set; } = "III";

        [Category("XIII. 排序演示")]
        [DisplayName("a.字母A")]
        public string SortParamA { get; set; } = "A";

        [Category("XIII. 排序演示")]
        [DisplayName("b.字母B")]
        public string SortParamB { get; set; } = "B";

        [Category("XIII. 排序演示")]
        [DisplayName("c.字母C")]
        public string SortParamC { get; set; } = "C";

        private static bool IsEnglish() =>
            !LocalizationManager.CurrentCulture.StartsWith("zh", StringComparison.OrdinalIgnoreCase);

        public string GetDisplayName(string propertyName)
        {
            if (!IsEnglish()) return null;
            return propertyName switch
            {
                "CameraName" => "Camera Name",
                "IsEnabled" => "Enabled",
                "CameraType" => "Camera Type",
                "InstallDate" => "Install Date",
                "WarrantyExpiry" => "Warranty Expiry",
                "BodyColor" => "Body Color",
                "LedColor" => "LED Color",
                "NightVisionColor" => "Night Vision Color",
                "Brightness" => "Brightness",
                "Contrast" => "Contrast",
                "Sharpness" => "Sharpness",
                "IpAddress" => "IP Address",
                "RtspPort" => "RTSP Port",
                "Username" => "Username",
                "Password" => "Password",
                "UseHttps" => "Use HTTPS",
                "Protocol" => "Protocol",
                "RecordDirectory" => "Record Directory",
                "SnapshotDirectory" => "Snapshot Directory",
                "ConfigImportFile" => "Config Import File",
                "LogFilePath" => "Log File Path",
                "NvrChannel" => "NVR Channel",
                "EnabledAnalyses" => "Enabled Analyses",
                "SliceMinutes" => "Slice Minutes",
                "Bitrate" => "Bitrate",
                "LoopRecording" => "Loop Recording",
                "MainZone" => "Main Zone",
                "RestrictedZone" => "Restricted Zone",
                "DutyPersonnel" => "Duty Personnel",
                "AlertEmails" => "Alert Emails",
                "CruisePoints" => "Cruise Points",
                "ZoneSensitivity" => "Zone Sensitivity",
                "AlarmLevels" => "Alarm Levels",
                "PresetCoordinates" => "Preset Coordinates",
                "NestedZoneConfig" => "Nested Zone Config",
                "ZoneAlerts" => "Zone Alerts",
                "NameFormula" => "Name Binding",
                "DurationFormula" => "Duration Binding",
                "BrightnessFormula" => "Brightness Binding",
                "RecordEnabledFormula" => "Record Binding",
                "SnapshotButton" => "Take Snapshot",
                "RebootButton" => "Reboot",
                "ExportButton" => "Export Config",
                "SnapshotCount" => "Snapshot Count",
                "RunState" => "Run State",
                "FirmwareVersion" => "Firmware Version",
                "HardwareSn" => "Hardware S/N",
                _ => null
            };
        }

        public string GetDescription(string propertyName)
        {
            if (!IsEnglish()) return null;
            return propertyName switch
            {
                "CameraName" => "Display name of the camera",
                "CameraType" => "Type of the camera",
                "IpAddress" => "IP address of the camera",
                "BodyColor" => "Main color of the camera housing",
                "LedColor" => "Indicator light color when running",
                "NightVisionColor" => "IR fill light color for night vision (nullable)",
                _ => null
            };
        }

        public string GetCategory(string propertyName)
        {
            if (!IsEnglish()) return null;
            return propertyName switch
            {
                "CameraName" or "IsEnabled" or "CameraType" or "InstallDate" or "WarrantyExpiry" => "I. Basic Info",
                "BodyColor" or "LedColor" or "NightVisionColor" or "Brightness" or "Contrast" => "II. Appearance",
                "IpAddress" or "RtspPort" or "Username" or "UseHttps" or "Protocol" => "III. Network",
                "RecordDirectory" or "SnapshotDirectory" or "ConfigImportFile" or "LogFilePath" => "IV. Storage",
                "NvrChannel" or "EnabledAnalyses" or "SliceMinutes" or "LoopRecording" => "V. Advanced",
                "MainZone" or "RestrictedZone" => "VI. Detection Zones",
                "DutyPersonnel" or "CruisePoints" => "VII. Collection Editor",
                "ZoneSensitivity" or "PresetCoordinates" or "NestedZoneConfig" => "VIII. Dictionary Editor",
                "NameFormula" or "DurationFormula" => "IX. Formula Binding",
                "SnapshotButton" or "RebootButton" or "ExportButton" or "SnapshotCount" => "X. Action Buttons",
                "FileSize" or "FocalLength" or "NetworkLatency" => "XI. ★ Host Extension",
                "FirmwareVersion" or "HardwareSn" => "XII. Read-Only & Hidden",
                "SortParam1" or "SortParam2" or "SortParam3" or "SortParam10" or "SortParamA" or "SortParamB" or "SortParamC" or "SortParamRoman1" or "SortParamRoman2" or "SortParamRoman3" => "XIII. Sort Demo",
                _ => null
            };
        }
    }

    // ==================== 辅助类 ====================
    [Serializable]
    public class DetectionZone
    {
        [Category("检测区域")]
        [DisplayName("区域名称")]
        public string Name { get; set; } = "";

        [Category("检测区域")]
        [DisplayName("灵敏度")]
        [Description("检测灵敏度（1-10）")]
        [DefaultValue(5)]
        [NumberSlider(1, 10, 1)]
        public int Sensitivity { get; set; } = 5;

        [Category("检测区域")]
        [DisplayName("启用AI分析")]
        [Description("是否在本区域启用AI分析")]
        public bool EnableAiAnalysis { get; set; } = true;

        public override string ToString() => $"区域: {Name}";
    }

    [Serializable]
    public class CruisePoint
    {
        [Category("巡航点位")]
        [DisplayName("预置位名称")]
        public string PresetName { get; set; } = "";

        [Category("巡航点位")]
        [DisplayName("水平角度")]
        [Description("云台水平角度（0-360）")]
        [DefaultValue(0)]
        [NumberSlider(0, 360, 1)]
        public int Horizontal { get; set; } = 0;

        [Category("巡航点位")]
        [DisplayName("垂直角度")]
        [Description("云台垂直角度（0-180）")]
        [DefaultValue(90)]
        [NumberSlider(0, 180, 1)]
        public int Vertical { get; set; } = 90;

        [Category("巡航点位")]
        [DisplayName("停留秒数")]
        [Description("到达预置位后停留时间（秒）")]
        [DefaultValue(5)]
        public int StaySeconds { get; set; } = 5;

        public override string ToString() => $"{PresetName} ({Horizontal}°,{Vertical}°)";
    }

    // ==================== 枚举定义 ====================

    public enum CameraType { Bullet, Dome, PTZ, Fisheye, Panoramic }

    public enum TransportProtocol { Rtsp, Onvif, Hikvision, Dahua, Rtmp }

    public enum RunState { Ready, Running, Recording, Warning, Error, Offline }

    // ==================== 其他演示类型 ====================

    [Serializable]
    public class SampleObject2
    {
        [Category("基本信息")]
        [DisplayName("创建时间")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        [Category("引用类型")]
        [DisplayName("子对象")]
        [CollectionEditor]
        public List<SampleObject2> Objects { get; set; } = new List<SampleObject2>();
        [Category("布局")]
        [DisplayName("亮度")]
        [DefaultValue(0.8)]
        [NumberSlider(0.0, 1.0, 0.1)]
        public FormulaBound<double> Brightness { get; set; } = new FormulaBound<double>();
        public override string ToString() => $"SampleObject2: {CreatedTime}";
    }

    public class SampleObject3
    {
        [Category("基本信息")]
        [DisplayName("创建时间")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        [Category("引用类型")]
        [DisplayName("嵌套配置绑定")]
        [FormulaEditor(typeof(DemoFormulaTreeProvider))]
        public FormulaBound<SampleObject2> NestedConfigFormula { get; set; } = new FormulaBound<SampleObject2> { Value = new SampleObject2() };
        public override string ToString() => $"SampleObject3: {CreatedTime}";
    }

    [Serializable]
    public class NestedConfig
    {
        [Category("嵌套配置")]
        [DisplayName("配置名称")]
        [FormulaEditor(typeof(DemoFormulaTreeProvider))]
        public FormulaBound<string> ConfigName { get; set; } = new FormulaBound<string> { Value = "默认配置" };
        [Category("嵌套配置")]
        [DisplayName("配置权重")]
        [FormulaEditor(typeof(DemoFormulaTreeProvider))]
        public FormulaBound<int> ConfigWeight { get; set; } = new FormulaBound<int> { Value = 100 };
        [Category("嵌套配置")]
        [DisplayName("普通属性")]
        public int PlainValue { get; set; } = 42;
    }

    public enum DockStyle { None, Top, Bottom, Left, Right, Fill }

    public enum HorizontalAlignmentType { Left, Center, Right, Stretch }

    public enum LogLevel { Debug, Info, Warning, Error, Fatal }

    public enum ProtocolType { Tcp, Udp, Http, WebSocket, Mqtt }
}
