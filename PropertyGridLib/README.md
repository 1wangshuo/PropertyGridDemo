# PropertyGridLib

WPF PropertyGrid 控件库 — 仿 WinForms PropertyGrid，基于 HandyControl UI 库。

> **版本 1.8.0** | .NET Framework 4.8+ / .NET 6/7/8/10-windows | HandyControl 3.5.1 | **作者：WangShuo**

### 📦 相关仓库

| 仓库 | 地址 |
|------|------|
| **本库（PropertyGridLib）** | NuGet：`Install-Package PropertyGridLib -Version 1.8.0` |
| **Demo 项目** | https://github.com/1wangshuo/PropertyGridDemo |
| **技能文档** | [skill/SKILL.md](https://github.com/1wangshuo/PropertyGridDemo/blob/main/skill/SKILL.md)（多页使用文档：基础用法 / 特性清单 / 编辑器类型 / 主题多语言 / 公式绑定 / PropertyGridPro / 示例模型 / FAQ） |

---

## ✨ v1.8.0 新特性

- ✅ **🎚️ 范围滑块（`RangeValue` + `[RangeSlider]`）** — 属性声明为 `RangeValue` 即自动映射成双滑块编辑器；`[RangeSlider(最小, 最大, 步长)]` 配边界与刻度。两端值实时写回，宿主直接读 `.Min` / `.Max` 两个 `double`。行内不常驻数字，按住拖动时滑块上方气泡实时显示「起点 ~ 终点」。
- ✅ **🧼 `RangeSliderControl` 可独立使用** — 不依赖属性网格，`Value` / `ValueStart` / `ValueEnd` 均可双向绑定。
- ✅ **📍 复位按钮 R 固定占位** — 未修改时隐藏但保留槽位，改值 / 拖动时 R 出现不再挤压值编辑器列（控件宽度不跳）。
- ✅ **🌳 文件夹选择对话框升级** — 圆角卡片树、悬停浅灰 / 选中主题色胶囊、常显细滚动条；文件夹项右键「新建文件夹 / 重命名 / 删除」；打开时自动滚动到当前路径节点。
- ✅ **✂️ 超长文本中间省略** — 绑定源树键名、类型提示只保留前 3 字 + 后 3 字，完整文本见 ToolTip，不再需要横向拉。
- ✅ **🌐 界面文本补齐多语言** — 标题默认值、集合 / 字典弹窗提示、紧凑摘要等硬编码中文全部改走本地化词典。

---

## ✨ v1.7.0 新特性

- ✅ **🧩 三个编辑器可独立使用** — 集合、字典、事件编辑器不再依赖 `PropertyGrid`，宿主直接给 `Items` / `Dictionary` / `HandlerName` 赋值即可嵌入任意容器。
- ✅ **📏 集合与字典编辑器新增 `Compact` 紧凑形态** — 只占一行（值框 + `...`），点 `...` 打开编辑弹窗；适合一行放一个集合的场景。
- ✅ **🔌 事件编辑器独立用法** — `HandlerName` / `IsBound` / `IsReadOnly` / `Placeholder` 四个依赖属性，请求编辑经 `RequestEditEvent` 路由事件抛出，由宿主决定打开哪种编辑器。
- ✅ **🪄 开关补齐** — `PropertyGrid.ShowHeader`（顶部标题区）、`PropertyGridPro.ShowDescription`（底部描述栏）、`PropertyCategory.ShowGroupSeparator`（分类头主题色细线）。
- ✅ **🌐 切换语言不乱序** — 属性与分类的排序改用特性原文作基准，本地化只换显示不换顺序。
- ✅ **🐛 Pro 滚动条抖动修复** — `PropertyGridPro.EnableVirtualization` 默认改为关闭：Pro 行高差异大（图片行约 200、文字行约 90），平均行高估算会让滚动条忽大忽小、底部留白。行数少于 `VirtualizationRowThreshold`（150）时一律全量生成。

---

## ✨ v1.6.0 新特性

- ✅ **🐛 「重置所有属性」闪退修复** — 颜色编辑器行内色块改用控件独占画刷（不再每次新建 / 释放画刷），修掉连点重置时布局阶段 `SolidColorBrush.ReleaseOnChannelCore` 抛 `0x8000FFFF (E_UNEXPECTED)` 导致进程退出的崩溃。三场景压测（虚拟化开 60 轮 / 虚拟化关 30 轮 / Pro 平铺面板 30 轮）全部通过。
- ✅ **🐛 复杂对象值区不再显示完整 CLR 类型名** — 可展开对象行（嵌套配置类）右侧此前显示默认 `ToString()` 的全名（含命名空间，如 `Ext.FlowWork.Settings.AutoBackupOptions`），又长又不可读。现按以下优先级显示：① 类型重写了 `ToString()` 的自定义输出保持不变；② 类型上声明 `[DisplayName("业务名")]` 时显示业务名；③ 回退友好类型名——去命名空间、展开泛型参数（`AutoBackupOptions`、`List<string>`）。枚举成员名、集合计数、颜色色值等原有显示均不受影响。
- ✅ **🎯 正式支持 .NET 10** — 多目标新增 `net10.0-windows` 并随 NuGet 包分发。

---

## ✨ v1.5.0 新特性

- ✅ **🐛 FormulaSwitchControl 重叠修复** — 可独立使用的「普通输入 ↔ 公式绑定」切换控件，此前进入公式模式时外层普通输入框未收起，与公式绑定条内部输入框叠在同一格（两个文本框重叠、🔗/ 挤压）。现改为随 `IsFormulaMode` 直接绑定 `Visibility` 互斥显隐，不再依赖 `ControlTemplate.Triggers`；两种状态下输入框、🔗 树选择、✕ 关闭排布正常。

  | 普通输入模式 | 公式绑定模式 |
  |---|---|
  | ![普通模式](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/formula-switch-normal.png) | ![公式模式](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/formula-switch-formula.png) |

---

## ✨ v1.4.0 新特性

- ✅ **📐 Small 紧凑布局（`TitlePlacement` 新属性）** — 属性行标题可在「编辑器上方（`Top`，默认）」与「编辑器左侧（`Left`）」之间切换；`Left` 时标题与编辑器平齐一行，对齐 HandyControl 表单左标题风格，行高压缩近一半。**R 复位按钮保留**在编辑器右侧，值被修改时标题右侧出现红点；公式绑定区与子属性列表仍另起一行
- ✅ **🅰️ 标题/分类头字体粗细（`TitleFontWeight` / `CategoryFontWeight`）** — 新增两个可配置属性，分别控制属性行标题（如 `1.设备名称`）与分类头（如 `I.基本信息`）的 `FontWeight`，默认 `Bold`，可按需设为 `Normal` / `Light` / `SemiBold` 等，解决 release 版本标题视觉上偏细的问题
- ✅ **⚡ 属性列表 UI 虚拟化** — 类别 Expander → 内层 ItemsControl 的两层嵌套压平为单层列表，开启虚拟滚动，只生成视口内行并复用容器。切换选中 → 属性页重建 **206ms → 8ms**，重复刷新托管堆增长 **44.5MB → 14.6MB**。新增 `EnableVirtualization` 开关（默认 `true`），遇自定义模板串值时可回退到全量生成
- ✅ **🔌 事件页能力** — `ShowEventsPage` 开关 + `EventPicker` 事件筛选选择器 + `EventItem` 模型 + `EventEditorControl` 行内编辑器；默认关闭，不影响旧行为，开启后便于设计器宿主把事件绑定与代码编辑器（如 CodeForge）打通
- ✅ **🐛 稳定性修复** — `SelectedObject` 置 null / 传入已销毁对象时不再崩溃；加载指示由 `LoadingCircle` 动画改为静态文本，消除动画时钟在快速显隐元素上的持续 tick

---

## ✨ v1.3.0 新特性

- ✅ **🧩 PropertyGridPro 平铺参数面板（新控件）** — 纵向平铺、卡片式属性行，面向「参数面板 / 属性面板」场景：`Header` / `HeaderDescription` / `SourceText` 头部区、`EditorWidth` 统一编辑器宽度、`RowTemplate` 可整体替换行模板；属性搜索附匹配计数与空结果提示；与 `PropertyGrid` 共用同一套特性、编辑器模板、本地化与主题资源，模型无需改动即可互换

  ![PropertyGridPro 平铺面板（浅色）](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-overview-light.png)

- ✅ **🃏 卡片式属性行布局** — 属性行改为卡片承载（圆角 7、行间距、分类头独立成卡片），底色取主题次级区域画刷（`SecondaryRegionBrush`），深浅主题下层次一致

  ![卡片式行布局](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-card-layout-detail.png)

- ✅ **🌓 主题色跟随（去除硬编码）** — 面板模板中硬编码的深色全部替换为 `SkinDark` / `ThemeHelper` 等主题资源键，卡片背景、边框、标题与描述文字、分类头、嵌套引导线均随主题切换

  ![主题跟随对比](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-theme-follow-window.png)

- ✅ **🪜 无限嵌套样式统一** — 复杂对象子属性以卡片逐层铺开并按层级缩进 + 引导线，层级不限；统一了各层级的间距、圆角与引导线样式

  ![多级嵌套](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-nested-4level-light.png)

- ✅ **🔗 公式绑定接入 Pro 面板** — `[FormulaEditor]` 属性在 `PropertyGridPro` 中同样显示公式绑定区（值编辑器 + 链接图标 + 公式文本框 + 树选择器 Popup），并支持全局 `FormulaTreeProvider`

  ![公式绑定行](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/formula-binding-row-dark.png)

- ✅ **📐 复位按钮常驻占位（消除布局抖动）** — 复位按钮可见性由 `Collapsed` 改为 `Hidden`（新增 `BoolToHiddenVisibilityConverter`），未悬停时仍保留占位，实测同行的值编辑器位移 **0px**，不再左右跳动

  ![复位按钮占位](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/reset-button-base-light.png)

- ✅ **🔀 [ToggleSwitch] 布尔开关特性（新特性）** — `bool` 专用编辑器：滑块式开关（圆角轨道 + 圆形滑块，HandyControl `ToggleButtonSwitch` 样式）＋可选的开/关状态文字（`[ToggleSwitch("已启用","已禁用")]` / `OnText` / `OffText`，传 `null` 可隐藏文字）；基类与 Pro 面板通过同一 `EditorSelector` 复用生效，未标注的 `bool` 属性保持默认开关外观（无状态文字）

  ![ToggleSwitch 布尔开关](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/toggle-switch-area-light.png)

- ✅ **🧹 演示程序清理** — 移除演示旧模型（右侧「实时值」区块与旧对象演示），Pro 演示窗口统一改用与基类一致的 `SampleObject`

---

## ✨ v1.2.0 新特性

- ✅ **🔌 宿主自定义编辑器扩展机制** — `PropertyGrid.RegisterEditor<TAttr>(DataTemplate)` 一行注册，宿主侧定义 Attribute + DataTemplate 即可让 PropertyGrid 渲染自定义编辑器；框架对此 Attribute 一无所知。内置 `CustomEditors` 静态字典 + `PropertyItem.CustomEditorTemplate` / `CustomEditorAttribute` 属性，`EditorTemplateSelector` 优先返回宿主模板，完全覆盖框架内置编辑器的渲染路径
- ✅ **🎨 颜色编辑器** — 自动识别 `Color` / `Color?` / `SolidColorBrush` 类型，显示为颜色预览块 + `...` 按钮（紧凑居左，与 FilePath/Dictionary 编辑器风格统一）；点击弹出 HandyControl `ColorPicker`（StaysOpen=true + 手动外部点击检测，支持拖动 Slider 不丢失焦点，点"确定"才应用值）；预览块显示 `#RRGGBB`（不透明时省略 Alpha 前缀）
- ✅ **📁 字典嵌套字典自动弹窗** — 字典值为集合（`IDictionary` / `IList`）时不再内联，改为显示"N 项 + ..."按钮，点击弹出二级编辑器（模态覆盖父级，UI 不会无限拉长）；字典值为复杂对象时仍内嵌 PropertyGrid 就地编辑
- ✅ **🌳 FormulaTree 类型提示** — `FormulaTreeNode` 新增 `FormulaType` / `TypeHint` 属性，TreeView 弹窗叶子节点右侧显示淡蓝色斜体类型名（如 `(int)` `(DateTime)`），宿主可通过 `ShowTypeHint=False` 关闭；Category 前缀改为罗马数字（I. II. III.），切换中英文时分类顺序稳定
- ✅ **🔤 全局字体同步** — `PropertyGrid.FontSize` / `FontFamily` 动态变化时通过 `DependencyPropertyDescriptor` 实时更新 `GlobalFontSize` / `GlobalFontFamily`，所有 Dialog（字典、集合、文件夹、颜色）打开时自动读取最新值
- ✅ **🌓 HandyControl 主题切换修复** — `SetAppSkin` 改为直接改 `Theme.Skin` 属性（HC 官方推荐方式），白→黑→白双向切换均正常，不再因替换 MergedDictionaries 导致重复 key 冲突

---

## ✨ v1.1.2 新特性

- ✅ **命令按钮编辑器 `[Button]`** — 属性标记后渲染为按钮，点击可反射调用宿主方法（`Action`）并触发 `ButtonClicked` 事件；新增静态 `PropertyGrid.GlobalButtonClicked` 全局事件，主网格与集合/字典内嵌网格中的按钮点击都能被宿主统一捕获
- ✅ **字典编辑器（IDictionary）** — 自动识别字典属性；新增 `DictionaryEditorDialog`（弹窗）与 `DictionaryEditorControl`（内联，当 `SelectedObject` 直接是字典时）；键、值均可为简单类型或复杂对象（内嵌 PropertyGrid，可无限嵌套），两者都是对象时左右各占 50%
- ✅ **修复字典 "..." 按钮无响应** 的历史缺陷（字典曾被当作集合但 EditCollection 只处理 IList）
- ✅ **修复嵌套网格中按钮"失效"**（内嵌 PropertyGrid 的按钮事件无法上抵宿主、且不刷新）
- ✅ **新增 .NET Core 3.0 / 3.1 目标框架**（连同 net48 / net6 / net7 / net8-windows）

---

## ✨ v1.1.1 新特性

- ✅ **集合编辑器内联显示** — 当 `PropertyGrid.SelectedObject` 设为 `IList` 类型时，自动内联显示集合编辑器（无需弹出对话框）
- ✅ **集合编辑器 UserControl 化** — `CollectionEditorControl` / `SimpleCollectionEditorControl` 可独立作为 UserControl 嵌入任意容器
- ✅ **FormulaBindingControl 独立使用** — 新增 `TreeItemsSource` / `FormulaTreeProvider` 依赖属性，支持脱离 PropertyGrid 直接使用
- ✅ **简单类型自动检测** — 内联模式自动区分简单类型（string/int/double 等）和复杂对象，选择对应编辑器

---

## ✨ v1.1.0 新特性

- ✅ **完全移除 System.Windows.Forms 依赖** - 纯 WPF 实现
- ✅ **多框架支持** - .NET Framework 4.8 / .NET 6/7/8-windows
- ✅ **HandyControl 主题集成** - 所有对话框自动跟随深色/浅色主题
- ✅ **自定义文件夹浏览器** - 纯 WPF 实现，树形目录浏览
- ✅ **文件夹浏览器支持手动输入路径自动定位** - 输入合法路径自动在树上选中
- ✅ **完整中英文多语言** - 所有新增组件支持语言切换
- ✅ **现代化序列化** - JSON 替代 BinaryFormatter
- ✅ **零依赖冲突** - System.Text.Json 向上兼容到 16.0+

---

## 目录

- [安装](#安装)
- [快速开始](#快速开始)
- [📚 技能文档（skill/）](https://github.com/1wangshuo/PropertyGridDemo/blob/main/skill/SKILL.md)
- [命名空间](#命名空间)
- [标准 .NET 特性](#标准-net-特性)
- [自定义特性](#自定义特性)
  - [FilePathAttribute — 文件路径选择](#filepathattribute--文件路径选择)
  - [DirectoryPathAttribute — 目录路径选择](#directorypathattribute--目录路径选择)
  - [CollectionEditorAttribute — 集合编辑器](#collectioneditorattribute--集合编辑器)
  - [NumberSliderAttribute — 数值滑块](#numbersliderattribute--数值滑块)
  - [RangeSliderAttribute — 范围滑块](#rangesliderattribute--范围滑块)
  - [FormulaEditorAttribute — 公式绑定](#formulaeditorattribute--公式绑定)
  - [MultiSelectAttribute — 多选列表](#multiselectattribute--多选列表)
  - [ButtonAttribute — 命令按钮](#buttonattribute--命令按钮)
  - [ToggleSwitchAttribute — 布尔开关](#toggleswitchattribute--布尔开关)
- [🎨 颜色编辑器](#-颜色编辑器)
- [📁 字典编辑器（IDictionary）](#-字典编辑器idictionary)
- [🔌 宿主自定义编辑器扩展机制](#-宿主自定义编辑器扩展机制)
- [🌳 FormulaTree 类型提示](#-formulatree-类型提示)
- [🧩 PropertyGridPro 平铺参数面板](#-propertygridpro-平铺参数面板)
- [编辑器类型总览](#编辑器类型总览)
- [PropertyGrid 集合模式（内联显示）](#propertygrid-集合模式内联显示)
- [集合编辑器 UserControl 独立使用](#集合编辑器-usercontrol-独立使用)
- [RangeSliderControl 独立使用](#rangeslidercontrol-独立使用)
- [FormulaBindingControl 独立使用](#formulabindingcontrol-独立使用)
- [FormulaBound&lt;T&gt; — 公式绑定泛型类型](#formulaboundt--公式绑定泛型类型)
- [TypeConverter 下拉列表](#typeconverter-下拉列表)
- [多语言 / 本地化系统](#多语言--本地化系统)
- [API 参考](#api-参考)
- [依赖项](#依赖项)
- [更新日志](#更新日志)

---

## 安装

### NuGet 包管理器

```powershell
Install-Package PropertyGridLib -Version 1.8.0
```

### .NET CLI

```bash
dotnet add package PropertyGridLib --version 1.8.0
```

### PackageReference (PackageReference)

```xml
<PackageReference Include="PropertyGridLib" Version="1.8.0" />
```

安装后，NuGet 会自动拉取依赖项 `HandyControl 3.5.1`。

### 🎯 单 DLL 部署

PropertyGridLib 使用 **Costura.Fody** 将 `HandyControl.dll`、`System.Text.Json.dll` 等依赖**嵌入合并到主 DLL** 内。

```
PropertyGridLib.dll  ← 唯一需要部署的 DLL（内部已嵌入 HandyControl 等）
```

宿主项目只需引用这一个 DLL，**无需额外附带 HandyControl.dll**。运行时通过 `AssemblyResolve` 事件自动从内嵌资源加载依赖。

> ⚠️ 若宿主项目也直接引用了 HandyControl（版本可能不同），两个内嵌的 HandyControl 会并存。建议宿主统一使用 PropertyGridLib 内嵌的版本，或在宿主 csproj 中显式指定 HandyControl 版本以覆盖内嵌。

---

## 快速开始

### 1. 添加命名空间

```xml
xmlns:pg="clr-namespace:PropertyGridLib;assembly=PropertyGridLib"
```

### 2. 在 XAML 中使用

```xml
<pg:PropertyGrid x:Name="propertyGrid"
                 Width="400"
                 Height="600"
                 ShowSearchBar="True"
                 ShowDescription="True"/>
```

### 3. 绑定对象

```csharp
propertyGrid.SelectedObject = new MyConfig();
```

### 4. 重置与刷新

```csharp
propertyGrid.ResetSelectedToDefault();  // 重置当前选中属性
propertyGrid.ResetAllToDefault();       // 重置所有属性
propertyGrid.RefreshProperties();        // 刷新属性列表
```

---

## 命名空间

| 命名空间 | 内容 |
|---------|------|
| `PropertyGridLib` | **PropertyGrid** 控件、**IPropertyLocalization** 接口 |
| `PropertyGridLib.Controls` | PropertyItem、IPropertyItem、PropertyCategory、EditorTemplateSelector、FormulaBound&lt;T&gt;、FormulaTreeNode、IFormulaTreeProvider、IMultiSelectProvider、MultiSelectControl、FormulaBindingControl、CollectionEditorControl、SimpleCollectionEditorControl、DictionaryEditorControl、ColorEditControl、PropertyButtonClickEventArgs、CustomEditorType 枚举 |
| `PropertyGridLib.Attributes` | FilePathAttribute、DirectoryPathAttribute、CollectionEditorAttribute、NumberSliderAttribute、FormulaEditorAttribute、MultiSelectAttribute、ButtonAttribute、ToggleSwitchAttribute |
| `PropertyGridLib.Dialogs` | FolderBrowserDialog、CollectionEditorDialog、SimpleCollectionEditorDialog、DictionaryEditorDialog |
| `PropertyGridLib.Localization` | LocalizationManager、LocalizationProxy、LocalizedExtension、Language 枚举 |
| `PropertyGridLib.Converters` | BoolToVisibilityConverter、InverseBoolConverter、ObjectToTypeConverter、ColorToBrushConverter 等值转换器 |

---

## 标准 .NET 特性

PropertyGrid 自动识别以下标准 .NET 特性：

| 特性 | 说明 | 示例 |
|------|------|------|
| `[Category("分类名")]` | 属性分组显示 | `[Category("外观")]` |
| `[DisplayName("显示名")]` | 自定义属性显示名称 | `[DisplayName("字体大小")]` |
| `[Description("描述")]` | 底部描述栏显示 | `[Description("文字的字体大小")]` |
| `[DefaultValue(value)]` | 默认值，控制重置按钮显示 | `[DefaultValue(14)]` |
| `[ReadOnly(true)]` | 只读属性，不可编辑 | `[ReadOnly(true)]` |
| `[Browsable(false)]` | 隐藏属性，不在列表中显示 | `[Browsable(false)]` |

---

## 自定义特性

### FilePathAttribute — 文件路径选择

标记属性为文件路径，显示浏览按钮，点击弹出文件选择对话框。支持多后缀筛选。

```csharp
using PropertyGridLib.Attributes;

// 单后缀
[FilePath("*.log")]
public string LogFilePath { get; set; } = "";

// 多后缀
[FilePath("*.json", "*.xml", "*.txt")]
public string ConfigFilePath { get; set; } = "";
```

**可选属性：**

| 属性 | 类型 | 说明 |
|------|------|------|
| `Filters` | `string[]` | 文件后缀列表（构造函数参数） |
| `Filter` | `string` | 完整过滤器字符串（如 `"*.json;*.xml"`） |
| `InitialDirectory` | `string` | 初始目录 |
| `Title` | `string` | 对话框标题 |
| `Multiselect` | `bool` | 是否允许多选 |

---

### DirectoryPathAttribute — 目录路径选择

标记属性为目录路径，显示浏览按钮，点击弹出文件夹选择对话框（纯 WPF 实现，支持 HandyControl 主题）。

```csharp
[DirectoryPath]
public string DataDirectory { get; set; } = "";
```

**可选属性：**

| 属性 | 类型 | 说明 |
|------|------|------|
| `InitialDirectory` | `string` | 初始目录 |
| `Title` | `string` | 对话框标题 |

**特性：**
- 树形目录浏览
- 支持路径直接输入
- 自动跟随 HandyControl 深色/浅色主题
- 完整的中英文多语言支持

---

### CollectionEditorAttribute — 集合编辑器

标记集合类型属性使用编辑器对话框。简单类型（string、int 等）使用简易编辑器，复杂对象使用完整编辑器（含属性面板）。

```csharp
// 简单类型集合 — 弹出简易编辑器（输入框 + 列表 + 增删排序）
[CollectionEditor]
public List<string> Tags { get; set; } = new List<string> { "A", "B" };

// 复杂对象集合 — 弹出完整编辑器（列表 + 属性面板 + 增删复制排序）
[CollectionEditor]
public List<RobotConfig> Robots { get; set; } = new List<RobotConfig>();
```

**可选属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `CanAdd` | `bool` | `true` | 是否允许添加新项 |
| `CanRemove` | `bool` | `true` | 是否允许删除项 |
| `CanCopy` | `bool` | `true` | 是否允许复制项 |
| `CanSort` | `bool` | `true` | 是否允许上移/下移排序 |

> 集合编辑器自动区分简单类型和复杂对象。无需手动指定，控件会根据 `List<T>` 的 `T` 类型自动选择合适的编辑器。

---

### NumberSliderAttribute — 数值滑块

将数值属性显示为滑块编辑器，支持 int、double、float、decimal 等数值类型。

```csharp
// 整数滑块
[NumberSlider(0, 100, 5)]  // min, max, step
public int Volume { get; set; } = 50;

// 小数滑块（默认显示只读数值标签）
[NumberSlider(0.0, 1.0, 0.1)]
public double Brightness { get; set; } = 0.8;

// 滑块 + 可编辑数字框（hc:NumericUpDown，可键入小数等精确值；min/max/step 与滑块共用，
// 双向同步；显示后取代只读数值标签）
[NumberSlider(0, 100, 5, ShowNumberBox = true)]
public double Opacity { get; set; } = 50;
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `minimum` | `double` | 最小值 |
| `maximum` | `double` | 最大值 |
| `step` | `double` | 步长（默认 1） |
| `ShowNumberBox` | `bool` | 是否显示可编辑数字输入框（默认 false；启用后与滑块双向同步并取代数值标签） |

---

### RangeSliderAttribute — 范围滑块

属性类型为 `RangeValue` 时自动映射为双滑块范围编辑器（不写特性即可用，默认 0 ~ 100 / 步长 1）；特性只用于配置边界与刻度。

```csharp
[RangeSlider(0, 1920, 8)]   // 最小值、最大值、步长
public RangeValue DetectHorizontalRange { get; set; } = new RangeValue(320, 1600);

// 不写特性：默认 0 ~ 100、步长 1
public RangeValue NightVisionGainRange { get; set; } = new RangeValue(20, 80);
```

宿主读取两端值（`double`）：

```csharp
RangeValue range = obj.DetectHorizontalRange;
double min = range.Min;   // 起点
double max = range.Max;   // 终点
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `minimum` | `double` | 最小值（默认 0） |
| `maximum` | `double` | 最大值（默认 100） |
| `step` | `double` | 步长 / 刻度（默认 1，滑块按刻度吸附） |

交互：行内不常驻数字；按住任一滑块拖动时，滑块上方气泡实时显示「起点 ~ 终点」，松手隐藏。值经 `Value`（`RangeValue`）双向写回宿主属性。

---

### FormulaEditorAttribute — 公式绑定

标记属性支持公式绑定编辑器。标记后，属性编辑器下方会额外显示一行公式绑定区（链接图标 + 公式文本框 + Popup 树选择器）。

```csharp
// 需要实现 IFormulaTreeProvider 接口
[FormulaEditor(typeof(MyFormulaTreeProvider))]
public FormulaBound<string> NameFormula { get; set; } = new FormulaBound<string> { Value = "默认值" };
```

**IFormulaTreeProvider 接口：**

```csharp
using PropertyGridLib.Controls;

public class MyFormulaTreeProvider : IFormulaTreeProvider
{
    public List<FormulaTreeNode> GetFormulaTree(PropertyItem propertyItem)
    {
        return new List<FormulaTreeNode>
        {
            new FormulaTreeNode
            {
                Header = "流程A",
                Children = new List<FormulaTreeNode>
                {
                    new FormulaTreeNode
                    {
                        Header = "任务1",
                        Children = new List<FormulaTreeNode>
                        {
                            new FormulaTreeNode { Header = "属性X", Formula = "&{流程A.任务1.属性X}" }
                        }
                    }
                }
            }
        };
    }
}
```

**FormulaTreeNode 属性：**

| 属性 | 类型 | 说明 |
|------|------|------|
| `Header` | `string` | 节点显示文本 |
| `Formula` | `string` | 选中后填入的公式字符串（仅叶子节点需要设置） |
| `Children` | `List<FormulaTreeNode>` | 子节点列表 |
| `FormulaType` | `Type?` | 叶子节点的数值类型（如 `typeof(int)`），用于 TreeView 弹窗显示类型提示 |
| `TypeHint` | `string` | 自动从 `FormulaType` 生成的类型提示（如 `(int)`、`(string)`） |

---

### MultiSelectAttribute — 多选列表

标记属性为多选列表编辑器，弹出 CheckBox 列表供用户多选。属性类型应为 `List<string>`，Provider 动态返回可选项。

```csharp
// 需要实现 IMultiSelectProvider 接口
[MultiSelect(typeof(MyMultiSelectProvider))]
public List<string> SelectedOptions { get; set; } = new List<string>();
```

**IMultiSelectProvider 接口：**

```csharp
using PropertyGridLib.Controls;

public class MyMultiSelectProvider : IMultiSelectProvider
{
    public List<string> GetAvailableItems(PropertyItem propertyItem)
    {
        return new List<string> { "选项A", "选项B", "选项C", "选项D" };
    }
}
```

### ButtonAttribute — 命令按钮

标记属性为命令按钮。被标记的属性不再显示值编辑器，而是渲染为一个按钮；点击后：① 通过反射调用宿主对象上 `Action` 指定的方法（若指定）；② 触发 `PropertyItem.ButtonClicked` 与 `PropertyGrid.ButtonClicked` / 静态 `PropertyGrid.GlobalButtonClicked` 事件。

```csharp
using PropertyGridLib.Attributes;

// 仅触发事件（文本回退到 DisplayName，随多语言切换）
[Button]
public object GreetButton { get; set; }

// 指定按钮文本 + 点击时反射调用的宿主方法（无参或单参）
[Category("操作")]
[DisplayName("测试连接")]
[Button("测试连接", nameof(TestConnection))]
public object TestConnectionButton { get; set; }

public void TestConnection() { /* 点击后由库反射调用 */ }
```

宿主侧统一响应（推荐订阅静态全局事件，可捕获集合/字典内嵌网格中的按钮）：

```csharp
PropertyGrid.GlobalButtonClicked += (s, e) =>
{
    // e.PropertyName / e.PropertyItem / e.Owner
    Console.WriteLine($"{e.PropertyName} 被点击");
};
// 静态事件，窗口关闭时记得取消订阅：PropertyGrid.GlobalButtonClicked -= handler;
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `text` | `string` | 按钮文本；为空时回退到 `DisplayName` |
| `action` | `string` | 点击时反射调用的宿主方法名（无参或单参）；为空时仅触发事件 |

> 建议将按钮属性声明为 `object`/`string` 占位类型（值可为 null）。按钮默认始终可点，仅当属性显式标注 `[ReadOnly(true)]` 时禁用。

---

### ToggleSwitchAttribute — 布尔开关

标注在 `bool` 属性上时，属性行的编辑器渲染为「滑块式开关（圆角轨道 + 圆形滑块）+ 开/关状态文字」（基于 HandyControl `ToggleButtonSwitch` 样式）；**未标注的 `bool` 属性保持默认显示**（同样是滑块开关，但不带状态文字）。该特性**仅对 `bool` 生效**，标注在其它类型属性上会被忽略。

```csharp
using PropertyGridLib.Attributes;

[Category("开关")]
[DisplayName("启用记录回放")]
[ToggleSwitch]                                     // 默认状态文字「开 / 关」
public bool LoopRecording { get; set; }

[ToggleSwitch("已启用", "已禁用")]                  // 自定义状态文字
public bool UseHttps { get; set; }

[ToggleSwitch(OnText = null, OffText = null)]       // 只显示开关，不显示文字
public bool IsEnabled { get; set; }
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `onText` | `string` | 打开（`true`）状态文字，默认「开」；为 `null` 或空字符串表示不显示文字 |
| `offText` | `string` | 关闭（`false`）状态文字，默认「关」；为 `null` 或空字符串表示不显示文字 |

> 两个参数也可作为命名参数单独设置，例如 `[ToggleSwitch(OnText = "在线")]`。
> 编辑器由 `EditorTemplateSelector.ToggleSwitchTemplate` 提供：宿主/派生控件可覆写该属性以替换开关外观；模板缺失时自动回退到默认 bool 编辑器，不会出现空白行。

实际渲染效果（浅色主题）：

| 未标注（默认 bool） | `[ToggleSwitch]` | `[ToggleSwitch("已启用","已禁用")]` |
|---|---|---|
| ![默认 bool](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/toggle-switch-row-plain-bool.png) | ![默认文字](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/toggle-switch-row-default.png) | ![自定义文字](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/toggle-switch-row-custom-text.png) |

| 开关区域（浅色） | 开关区域（深色） |
|---|---|
| ![浅色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/toggle-switch-area-light.png) | ![深色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/toggle-switch-area-dark.png) |

---

## 🎨 颜色编辑器

### 自动检测

PropertyGrid 自动识别以下类型，无需任何特性：

| 类型 | 处理方式 |
|------|---------|
| `Color` | 值类型，双向编辑 |
| `Color?` | 可空，支持 null |
| `SolidColorBrush` | 取 `Brush.Color` 编辑，写回时重建 SolidColorBrush |

### UI 布局

```
┌──────────────────┐ ┌──────┐
│ ■ #FF0000        │ │  ... │
└──────────────────┘ └──────┘
   颜色预览块 + HEX      编辑按钮
```

- 居左占满宽度，与 FilePath/Dictionary 编辑器风格统一
- 预览块：16×16 颜色方块 + `#AARRGGBB` HEX 文本
- **不透明颜色**（Alpha=255）时省略 Alpha 前缀，显示 `#RRGGBB`
- 深色/浅色主题自动适配

### 交互流程

点击 `...` 按钮弹出 HandyControl `ColorPicker`：

| 操作 | 行为 |
|------|------|
| 拖动 Slider / 选色块 | ColorPicker 内部预览，**不修改** PropertyItem.Value |
| 点"确定" | 写入 PropertyItem.Value → 关闭弹窗 |
| 点"取消" | 不改值 → 关闭弹窗 |
| 点弹窗外面 | 不改值 → 关闭弹窗 |

### 关键技术点

- `StaysOpen=true` + 手动 `Window.PreviewMouseDown` 检测外部点击（避免 HC ColorPicker 内部 Slider 拖动导致 Popup 误判关闭）
- `IsInVisualTree()` 从 OriginalSource 向上遍历 VisualTree 判定点击来源
- `Freeze()` 创建 SolidColorBrush 防止绑定循环警告
- `DependencyPropertyDescriptor` 监听 `FontSize`/`FontFamily` 变化，ColorPicker 弹窗自动同步宿主字体

### FormulaBound 属性的颜色提示

当属性带 `[FormulaEditor]` 且 `ShowTypeHint=True` 时，公式绑定区右侧显示淡蓝色斜体类型提示（如 `(string)`、`(int)`），颜色与公式输入框区分。

---

## 📁 字典编辑器（IDictionary）

字典属性（`Dictionary<TKey,TValue>` 等 `IDictionary`）会被自动识别，显示为 "N 项 + ..." 按钮，点击弹出 `DictionaryEditorDialog`。无需任何特性；若把 `[CollectionEditor]` 标在字典属性上，也会路由到字典编辑器。

```csharp
// 简单键值：键、值都用文本框编辑（自动类型转换）
public Dictionary<string, int> ScoreMap { get; set; } = new Dictionary<string, int>();

// 复杂值：值区显示内嵌 PropertyGrid，可继续编辑其内部的集合/字典（无限嵌套）
public Dictionary<string, RobotConfig> Robots { get; set; } = new Dictionary<string, RobotConfig>();

// 复杂键：键区也显示内嵌 PropertyGrid（键类型需重写 Equals/GetHashCode）
public Dictionary<ServerEndpoint, string> Endpoints { get; set; } = new Dictionary<ServerEndpoint, string>();

// ===== 嵌套字典：值为字典时自动弹窗 =====
// 框架检测到值类型是 IDictionary → 显示 "N 项 ..." 按钮，点击弹出二级字典编辑器
public Dictionary<string, Dictionary<string, int>> NestedConfig { get; set; } = new()
{
    { "白天模式", new Dictionary<string, int> { { "人员检测", 6 }, { "车辆检测", 5 } } },
    { "夜间模式", new Dictionary<string, int> { { "人员检测", 8 }, { "车辆检测", 7 } } }
};

// ===== 字典值为列表：同样弹窗 =====
public Dictionary<string, List<string>> ZoneAlerts { get; set; } = new()
{
    { "大门", new List<string> { "2024-03-15 检测到人员" } }
};
```

### 嵌套值的三种渲染模式

| 值类型 | 检测 | UI 表现 | 编辑方式 |
|--------|------|---------|---------|
| 简单类型（string/int/double 等） | `IsSimpleType` | TextBox | 就地文本编辑 |
| 集合类型（IDictionary / IList） | `IsDictionaryType` / `IsListType` | **"N 项 ..."按钮** | 点击弹窗（模态覆盖父级） |
| 其他复杂对象 | 排除以上两种 | 内嵌 PropertyGrid | 就地编辑（可继续嵌套其属性） |

### 内联模式

当 `PropertyGrid.SelectedObject` 直接设为字典时，自动内联显示 `DictionaryEditorControl`（就地编辑、实时写回，无弹窗），与集合内联模式一致。

```csharp
propertyGrid.SelectedObject = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
```

### 独立使用

`DictionaryEditorControl`（`PropertyGridLib.Controls`）可作为 UserControl 嵌入任意容器，`Items` 为 `IDictionary`，直接操作源字典。

> ⚠️ 复杂键为可变对象时，就地编辑会改变其哈希——编辑器在"确定/切换条目"时会 `Clear + 重新 Add` 重建哈希；复杂键类型务必重写 `Equals`/`GetHashCode`。另注意 `System.Text.Json` 不支持非字符串字典键，故复杂键字典不宜作为会被深拷贝（Reset）的 `[Serializable]` 模型属性。

### 紧凑形态（Compact）

`Compact="True"` 时只显示一行：值框 + `...`。点 `...` 打开与网格行同款的编辑弹窗，关闭后刷新摘要。默认 `False`，保持完整内联编辑面板。

```xml
<controls:DictionaryEditorControl x:Name="dict" Items="{Binding MyMap}" Compact="True" />
```

---

## 🔌 宿主自定义编辑器扩展机制

PropertyGridLib 提供通用扩展点，宿主只需 3 步即可让自定义编辑器生效，框架对此 Attribute 一无所知。

### 完整流程

```
宿主侧: [MyUnitEditor("GB,MB,KB", "MB")] 属性
  → PropertyGrid.RegisterEditor<MyUnitEditorAttribute>(template)  // 注册
    → PropertyItem.DetectCustomEditor 发现 Attribute
      → CustomEditorTemplate = template, CustomEditorAttribute = attr  // 存起来
        → EditorTemplateSelector.SelectTemplate 优先返回 CustomEditorTemplate  // 渲染
          → DataTemplate 里 NumericUpDown + ComboBox
            → ComboBox Loaded 从 CustomEditorAttribute 读参数  // 宿主解析参数
```

### Step 1 — 宿主定义 Attribute

```csharp
// 纯宿主侧，框架对此类型一无所知
[AttributeUsage(AttributeTargets.Property)]
public class MyUnitEditorAttribute : Attribute
{
    public string Units { get; }
    public string DefaultUnit { get; set; }

    public MyUnitEditorAttribute(string units) => Units = units;
}
```

### Step 2 — 宿主定义 DataTemplate

```xml
<!-- App.xaml 或任意宿主资源字典 -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 必须先合并 PropertyGridLib 的 Generic.xaml -->
            <ResourceDictionary Source="pack://application:,,,/PropertyGridLib;component/Themes/Generic.xaml"/>
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml"/>
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/Theme.xaml"/>
        </ResourceDictionary.MergedDictionaries>

        <DataTemplate x:Key="MyUnitEditorTemplate">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                <hc:NumericUpDown Grid.Column="0"
                                  Value="{Binding ValueString, ...}"
                                  Margin="0,0,4,0"/>
                <ComboBox Grid.Column="1"
                          Loaded="UnitCombo_Loaded"
                          SelectionChanged="UnitCombo_SelectionChanged"/>
            </Grid>
        </DataTemplate>
    </ResourceDictionary>
</Application.Resources>
```

### Step 3 — 宿主注册

```csharp
// MainWindow 构造函数中（必须在 PropertyGrid.SelectedObject 赋值前注册）
PropertyGridLib.PropertyGrid.RegisterEditor<MyUnitEditorAttribute>(
    (DataTemplate)FindResource("MyUnitEditorTemplate"));
```

### Step 4 — 使用

```csharp
[MyUnitEditor("GB,MB,KB", "MB")]
public double FileSize { get; set; } = 256;
```

### 模板内读 Attribute 参数

宿主在 DataTemplate 的 code-behind 中，从 `PropertyItem.CustomEditorAttribute` 拿到宿主 Attribute 实例，再读参数：

```csharp
private void UnitCombo_Loaded(object sender, RoutedEventArgs e)
{
    if (sender is ComboBox combo && combo.DataContext is PropertyItem pi)
    {
        var attr = pi.CustomEditorAttribute as MyUnitEditorAttribute;
        if (attr == null) return;

        var units = (attr.Units ?? "").Split(',').Select(s => s.Trim()).ToList();
        combo.ItemsSource = units;
        combo.SelectedItem = !string.IsNullOrEmpty(attr.DefaultUnit)
            ? attr.DefaultUnit
            : units.FirstOrDefault();
    }
}
```

### API 参考

| 成员 | 类型 | 说明 |
|------|------|------|
| `PropertyGrid.CustomEditors` | `Dictionary<Type, DataTemplate>` | 静态注册字典，宿主可直接操作或通过 `RegisterEditor` 泛型方法 |
| `PropertyGrid.RegisterEditor<TAttribute>(template)` | `static void` | 泛型注册方法，一行搞定 |
| `PropertyItem.CustomEditorTemplate` | `DataTemplate?` | 宿主注册的编辑器模板，非 null 时 EditorTemplateSelector 优先使用 |
| `PropertyItem.CustomEditorAttribute` | `Attribute?` | 宿主 Attribute 实例，模板 code-behind 可读取参数 |

### 优先级

宿主注册 **高于** 框架内置。`DetectCustomEditor` 最前面先查 `PropertyGrid.CustomEditors`，找到就返回，内置的 Slider/Color/FilePath/Dictionary 都排在后面。宿主甚至可以覆盖框架内置编辑器的行为 — 比如给带 `[NumberSlider]` 的属性也注册一个自己的模板。

---

## 🌳 FormulaTree 类型提示

`FormulaTreeNode` 支持在 TreeView 弹窗中显示叶子节点的类型信息，帮助用户判断绑定源的类型是否匹配目标属性。

### 宿主设置类型

```csharp
var provider = new DemoFormulaTreeProvider();
// 叶子节点设置 FormulaType
new FormulaTreeNode
{
    Header = "任务A.值",
    Formula = "&{Flow2,TaskA,Value}",
    FormulaType = typeof(int)  // ← 宿主指定类型
}
```

### PropertyGrid 控制显示

```xml
<pg:PropertyGrid ShowTypeHint="False" .../>
```

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ShowTypeHint` | `bool` | `true` | 是否显示类型提示（FormulaTree 弹窗 + 公式绑定区） |

### 效果

```
├─ ▼ 流程2
│    任务A.值          (int)    ← 淡蓝色斜体，靠右
│    任务B.计数        (double)
│    任务C.启用状态    (bool)
```

- 非叶子节点（分类容器）不填 `FormulaType`，不显示类型提示
- `TypeHint` 自动从 `FormulaType` 生成，宿主也可直接覆盖 `TypeHint` 属性
- 颜色 `InfoBrush` + `Italic` + 比正常小 2 号，与节点 Header 区分

---

## 🧩 PropertyGridPro 平铺参数面板

`PropertyGridPro` 是 1.3.0 新增的控件，与 `PropertyGrid` **同源**：共用同一套特性体系、编辑器模板、本地化与主题资源，模型代码无需任何改动即可在两者之间切换；区别在于布局形态与面向场景。

| 对比项 | `PropertyGrid` | `PropertyGridPro` |
|--------|----------------|-------------------|
| 整体布局 | 分类 + 可折叠树形层级（仿 WinForms） | 纵向平铺，属性与分组直接铺开，列表整体滚动 |
| 属性行 | 通栏式行 | 卡片式行（圆角、行间距、分类头卡片） |
| 头部区 | 无 | `Header` + `HeaderDescription` + `SourceText` |
| 属性搜索 | 有（过滤属性行） | 有（过滤 + 匹配计数 + 空结果提示） |
| 编辑器宽度 | 随行自适应 | `EditorWidth` 统一控制，行内编辑器对齐 |
| 行模板 | 内置 | `RowTemplate` 可整体替换 |
| 适用场景 | 层级深、属性多、需要折叠收纳 | 参数面板 / 属性面板，强调一屏扫读 |

| PropertyGridPro（浅色） | PropertyGridPro（深色） |
|---|---|
| ![Pro 浅色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-overview-light.png) | ![Pro 深色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-overview-dark.png) |

### 用法

```xml
<pg:PropertyGridPro x:Name="PropertyGridPro1"
                    Header="参数配置"
                    HeaderDescription="修改后立即生效"
                    SourceText="MyApp.Models.SampleObject"
                    ShowHeader="True"
                    ShowSearchBar="True"
                    ShowDescription="True"
                    EditorWidth="220"/>
```

```csharp
PropertyGridPro1.SelectedObject = new SampleObject();
```

### 依赖属性与方法

| 成员 | 类型 | 说明 |
|------|------|------|
| `SelectedObject` | `object` | 要编辑的目标对象（与 `PropertyGrid` 一致） |
| `Header` | `string` | 面板标题 |
| `HeaderDescription` | `string` | 标题下的说明文字 |
| `SourceText` | `string` | 来源说明（如模型类型全名） |
| `ShowHeader` | `bool` | 是否显示头部区（默认 `true`） |
| `ShowSearchBar` | `bool` | 是否显示属性搜索栏（默认 `true`） |
| `ShowDescription` | `bool` | 是否显示底部描述栏（默认 `true`） |
| `EditorWidth` | `double` | 行内编辑器统一宽度 |
| `RowTemplate` | `DataTemplate` | 整行卡片模板，可整体替换外观 |
| `RefreshProperties()` | 方法 | 刷新属性列表 |
| `ResetAllToDefault()` / `ResetSelectedToDefault()` | 方法 | 重置全部 / 当前选中属性 |
| `SetSubPropertiesExpanded(bool)` | 方法 | 批量展开或折叠所有嵌套子属性 |

### 卡片式行布局与无限嵌套

属性行以卡片承载：圆角 7、行间距、分类头独立成卡片，卡片底色取主题次级区域画刷（`SecondaryRegionBrush`）；复杂对象的子属性逐层铺开，按层级增加缩进与引导线，层级不限。

| 卡片式行细节 | 多级嵌套（浅色） | 多级嵌套（深色） |
|---|---|---|
| ![卡片细节](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-card-layout-detail.png) | ![嵌套浅色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-nested-4level-light.png) | ![嵌套深色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-nested-dark.png) |

### 搜索

| 命中（浅色） | 无匹配（浅色） | 命中（深色） | 无匹配（深色） |
|---|---|---|---|
| ![命中](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-search-light.png) | ![空结果](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-search-empty-light.png) | ![命中深色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-search-dark.png) | ![空结果深色](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/pro-search-empty-dark.png) |

> 完整用法（含公式绑定、布尔开关在 Pro 中的表现、主题与多语言）见 [skill/06-PropertyGridPro-平铺参数面板.md](https://github.com/1wangshuo/PropertyGridDemo/blob/main/skill/06-PropertyGridPro-%E5%B9%B3%E9%93%BA%E5%8F%82%E6%95%B0%E9%9D%A2%E6%9D%BF.md)。

---

## 编辑器类型总览

控件根据属性类型和特性自动选择编辑器模板：

| 编辑器 | 触发条件 | 显示效果 |
|--------|---------|---------|
| TextBox | `string` 类型 | 文本输入框 |
| ToggleSwitch（默认 bool） | `bool` / `bool?` | 滑块式开关（圆角轨道 + 圆形滑块，无状态文字） |
| **ToggleSwitch（带文字）** | `[ToggleSwitch]` 特性（`bool`） | **滑块式开关 + 开/关状态文字（文字可自定义或隐藏）** |
| NumericUpDown | `int`, `double`, `float`, `decimal`, `long`, `short`, `byte` | 数字输入框（带增减按钮） |
| ComboBox | `enum` | 枚举下拉选择 |
| DateTimePicker | `DateTime` / `DateTime?` | 日期时间选择器 |
| Slider | `[NumberSlider]` 特性 | 滑块 + 数值标签 |
| **RangeSlider** | `RangeValue` 类型（`[RangeSlider]` 配边界与步长） | **双滑块区间选择，拖动时气泡显示「起点 ~ 终点」；宿主读 `.Min` / `.Max`** |
| **Color** | `Color` / `Color?` / `SolidColorBrush` | **颜色预览块 + "..." 按钮（弹出 HC ColorPicker）** |
| FilePath | `[FilePath]` 特性 | 文本框 + 浏览按钮 |
| DirectoryPath | `[DirectoryPath]` 特性 | 文本框 + 浏览按钮 |
| Collection | `[CollectionEditor]` 特性或 `IList` 类型 | 文本框 + 编辑按钮（弹出对话框） |
| **Dictionary** | `IDictionary` 类型（自动识别） | "N 项" + 编辑按钮；**值为集合时显示 "N 项 ..." 弹窗** |
| DropDown | `TypeConverter` 标准值 | 下拉列表（支持排他/可编辑模式） |
| MultiSelect | `[MultiSelect]` 特性 | 摘要文本 + 弹出 CheckBox 列表 |
| Button | `[Button]` 特性 | 命令按钮（点击触发宿主方法/事件） |
| Expandable | 复杂对象（非简单类型、非集合） | 可展开子属性 |
| Formula | `[FormulaEditor]` 特性或 `FormulaBound<T>` | 值编辑器 + 公式绑定区 |
| **宿主自定义** | `PropertyGrid.CustomEditors` 注册的 Attribute | 宿主提供的 DataTemplate |

---

## PropertyGrid 集合模式（内联显示）

当 `PropertyGrid.SelectedObject` 设为 `IList` 类型（如 `List<string>`、`List<CustomType>`）时，PropertyGrid 自动切换为集合编辑模式，内联显示集合编辑器，无需弹出对话框。

```csharp
// 简单类型列表 — 自动显示 SimpleCollectionEditorControl
propertyGrid.SelectedObject = new List<string> { "A", "B", "C" };

// 复杂对象列表 — 自动显示 CollectionEditorControl（含内嵌 PropertyGrid）
propertyGrid.SelectedObject = new List<MyConfig> { new MyConfig() };
```

**自动类型检测：**
- 简单类型（`string`、`int`、`double`、`float`、`decimal`、`long`、`bool`、`DateTime`、`enum` 等）→ `SimpleCollectionEditorControl`
- 复杂对象类型 → `CollectionEditorControl`（左侧列表 + 右侧属性面板）
- `string` 不会被识别为集合；`IDictionary` 会走独立的字典编辑器（见上节）

---

## 集合编辑器 UserControl 独立使用

`CollectionEditorControl` 和 `SimpleCollectionEditorControl` 现在可以作为 UserControl 独立嵌入任意容器，不再仅限于对话框弹出。

### CollectionEditorControl（复杂对象列表）

```xml
xmlns:controls="clr-namespace:PropertyGridLib.Controls;assembly=PropertyGridLib"

<controls:CollectionEditorControl x:Name="editor" Items="{Binding MyList}" />
```

```csharp
editor.Items = new List<MyConfig>
{
    new MyConfig { Name = "Item1", Value = 100 },
    new MyConfig { Name = "Item2", Value = 200 }
};
```

### SimpleCollectionEditorControl（简单类型列表）

```xml
<controls:SimpleCollectionEditorControl x:Name="simpleEditor" Items="{Binding MyStringList}" />
```

```csharp
simpleEditor.Items = new List<string> { "Apple", "Banana", "Cherry" };
// ElementType 会自动从集合泛型参数推断，也可手动指定
simpleEditor.ElementType = typeof(int);
simpleEditor.Items = new List<int> { 1, 2, 3 };
```

> 两个控件都直接操作传入的 `IList` 实例，每次增删改操作后立即同步回源集合。

### 紧凑形态（Compact）

`CollectionEditorControl` 与 `DictionaryEditorControl` 支持 `Compact="True"`：只占一行（值框 + `...`），点 `...` 打开编辑弹窗，关闭后刷新摘要。默认 `False`，保持完整内联编辑面板。

```xml
<controls:CollectionEditorControl x:Name="editor" Items="{Binding MyList}" Compact="True" />
```

---

## 事件编辑器独立使用

`EventEditorControl` 可脱离 `PropertyGrid` 使用，直接给依赖属性赋值即可：

```xml
<controls:EventEditorControl x:Name="ev"
                             HandlerName="{Binding Path, Mode=TwoWay}"
                             IsBound="{Binding HasHandler}"
                             Placeholder="选择处理方法" />
```

| 属性 | 说明 |
|------|------|
| `HandlerName` | 处理方法名。默认双向绑定；`DataContext` 为 `EventItem` 时与它的 `HandlerName` 自动同步 |
| `IsBound` | 是否已绑定处理方法（独立用法下可用于绿标或自定义提示，控件模板不直接渲染） |
| `IsReadOnly` | 只读。独立用法直接设置；网格内联时同步自 `EventItem` |
| `Placeholder` | 输入框占位文字。为 null 时回落到本地化的「未绑定」 |

请求编辑经 `RequestEditEvent` 路由事件抛出，参数携带对应的 `EventItem`（独立用法下为 null，宿主可从发送方读 `HandlerName`）。宿主收到后自行决定打开哪种编辑器。

---

## RangeSliderControl 独立使用

`RangeSliderControl` 可脱离 `PropertyGrid` 使用，直接给依赖属性赋值 / 双向绑定：

```xml
<controls:RangeSliderControl Minimum="0" Maximum="1920" Step="8"
                             Value="{Binding Range, Mode=TwoWay}" />
```

| 属性 | 说明 |
|------|------|
| `Value` | `RangeValue` 整体区间（默认双向绑定） |
| `ValueStart` / `ValueEnd` | 两端值（`double`，默认双向绑定） |
| `Minimum` / `Maximum` / `Step` | 边界与刻度（默认 0 / 100 / 1） |
| `IsReadOnly` | 只读时滑块禁用 |

拖动时气泡显示「起点 ~ 终点」，松手隐藏；读值用 `Value.Min` / `Value.Max`（或 `ValueStart` / `ValueEnd`）。

---

## FormulaBindingControl 独立使用

`FormulaBindingControl` 支持脱离 PropertyGrid 独立使用，通过设置 `TreeItemsSource` 或 `FormulaTreeProvider` 提供公式树数据。

### 方式一：FormulaTreeProvider（推荐）

```xml
<!-- 定义 Provider -->
<Window.Resources>
    <local:MyFormulaTreeProvider x:Key="myProvider"/>
</Window.Resources>

<controls:FormulaBindingControl
    FormulaTreeProvider="{StaticResource myProvider}"
    FormulaString="{Binding MyFormula, Mode=TwoWay}" />
```

### 方式二：TreeItemsSource（直接绑定树节点）

```xml
<controls:FormulaBindingControl
    TreeItemsSource="{Binding MyTreeNodes}"
    FormulaString="{Binding MyFormula, Mode=TwoWay}" />
```

```csharp
public List<FormulaTreeNode> MyTreeNodes { get; } = new List<FormulaTreeNode>
{
    new FormulaTreeNode
    {
        Header = "传感器",
        Children =
        {
            new FormulaTreeNode { Header = "温度", Formula = "&{Sensors,Temperature,Value}" },
            new FormulaTreeNode { Header = "湿度", Formula = "&{Sensors,Humidity,Value}" }
        }
    }
};
```

**数据源优先级：** `TreeItemsSource` > `FormulaTreeProvider` > `PropertyItem.DataContext`（PropertyGrid 内嵌用法）

---

## FormulaBound&lt;T&gt; — 公式绑定泛型类型

`FormulaBound<T>` 是一个包装类型，同时持有值和公式字符串。用于数据绑定场景，让属性既能存储实际值，又能记录绑定公式。

```csharp
using PropertyGridLib.Controls;

public class MyConfig
{
    // string 类型公式绑定
    [FormulaEditor(typeof(MyProvider))]
    public FormulaBound<string> Name { get; set; } = new FormulaBound<string> { Value = "默认", Formula = "" };

    // int 类型也支持公式绑定
    [FormulaEditor(typeof(MyProvider))]
    public FormulaBound<int> Count { get; set; } = new FormulaBound<int> { Value = 42 };
}
```

**FormulaBound&lt;T&gt; 属性：**

| 属性 | 类型 | 说明 |
|------|------|------|
| `Value` | `T` | 实际值 |
| `Formula` | `string` | 绑定公式字符串（如 `&{流程.任务.属性}`） |

> PropertyGrid 自动检测 `FormulaBound<T>` 类型，使用内部类型 `T` 选择编辑器，同时显示公式绑定区。即使不标记 `[FormulaEditor]`，`FormulaBound<T>` 也会自动启用公式绑定输入框。

---

## TypeConverter 下拉列表

当属性的 `TypeConverter` 提供标准值（`GetStandardValuesSupported` 返回 `true`），PropertyGrid 自动显示为下拉列表。

```csharp
using System.ComponentModel;

// 自定义 TypeConverter 提供标准值
public class ComPortListConverter : TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;  // true=排他（不可手动输入），false=可输入

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        return new StandardValuesCollection(new[] { "COM1", "COM2", "COM3", "COM4" });
    }
}

// 使用
[TypeConverter(typeof(ComPortListConverter))]
public string ComPort { get; set; } = "COM1";
```

---

## 多语言 / 本地化系统

PropertyGridLib 内置中英文双语支持，提供三个层面的本地化能力：

### 1. 类库内部字符串

通过 `LocalizationManager` 管理搜索框占位符、对话框标题、按钮文本等 47 个内置字符串。

```csharp
using PropertyGridLib.Localization;

// 切换语言
LocalizationManager.CurrentLanguage = Language.EnUS;   // 英文
LocalizationManager.CurrentLanguage = Language.ZhCN;   // 中文（默认）

// 获取本地化字符串
var text = LocalizationManager.GetString("SearchPlaceholder");
var count = LocalizationManager.GetString("SelectedCount", 5);
```

在 XAML 中使用标记扩展绑定：

```xml
xmlns:loc="clr-namespace:PropertyGridLib.Localization;assembly=PropertyGridLib"

<TextBlock Text="{loc:Localized SearchPlaceholder}"/>
```

### 2. 属性名 / 描述 / 分类动态翻译

实现 `IPropertyLocalization` 接口，让数据对象提供动态的多语言翻译。语言切换时，PropertyGrid 自动刷新所有属性。

```csharp
using PropertyGridLib;
using PropertyGridLib.Localization;

public class MyConfig : IPropertyLocalization
{
    [Category("外观")]
    [DisplayName("名称")]
    [Description("对象的显示名称")]
    public string Name { get; set; } = "Test";

    // 返回 null 时回退到 [DisplayName] 特性值
    public string GetDisplayName(string propertyName, Language language)
    {
        if (language == Language.ZhCN) return null;  // 中文用特性值
        return propertyName switch
        {
            "Name" => "Name",
            _ => null
        };
    }

    // 返回 null 时回退到 [Description] 特性值
    public string GetDescription(string propertyName, Language language)
    {
        if (language == Language.ZhCN) return null;
        return propertyName switch
        {
            "Name" => "Display name of the object",
            _ => null
        };
    }

    // 返回 null 时回退到 [Category] 特性值
    public string GetCategory(string propertyName, Language language)
    {
        if (language == Language.ZhCN) return null;
        return propertyName switch
        {
            "Name" => "Appearance",
            _ => null
        };
    }
}
```

### 3. 语言切换流程

```
用户调用 LocalizationManager.CurrentLanguage = Language.EnUS
    ↓
触发 LanguageChanged 事件
    ↓
PropertyGrid.OnLanguageChanged() 遍历所有属性
    ↓
PropertyItem.UpdateLocalization() 递归更新 DisplayName/Description/Category
    ↓
刷新描述栏 + 重新分组过滤
```

**内置本地化 Key 列表（部分常用，共 47 个）：**

| Key | 中文 | English |
|-----|------|---------|
| SearchPlaceholder | 搜索属性... | Search properties... |
| SelectBindingSource | 选择绑定源 | Select Binding Source |
| SelectItems | 选择项 | Select Items |
| SelectMultiple | 选择多项 | Select Multiple |
| ResetToDefault | 重置为初始值 | Reset to default |
| FormulaPlaceholder | 公式绑定... | Formula binding... |
| SelectFile | 选择文件 | Select File |
| SelectFolder | 选择文件夹 | Select Folder |
| AllFiles | 所有文件 | All Files |
| Files | 文件 | Files |
| ItemsCount | [{0} 项] | [{0} items] |
| Misc | 杂项 | Miscellaneous |
| NotSelected | （未选择） | (None) |
| SelectedCount | 已选 {0} 项 | {0} items selected |
| CollectionEditorTitle | 集合编辑器 | Collection Editor |
| CollectionItems | 集合项 | Collection Items |
| Properties | 属性 | Properties |
| Add | 添加 | Add |
| Copy | 复制 | Copy |
| Remove | 删除 | Remove |
| OK | 确定 | OK |
| Cancel | 取消 | Cancel |
| EditCollectionTitle | 编辑集合 | Edit Collection |
| InputPlaceholder | 输入新值... | Enter new value... |
| MoveUp | 上移 | Move Up |
| MoveDown | 下移 | Move Down |
| Tip | 提示 | Tip |
| Error | 错误 | Error |
| AddFailed | 添加失败：{0} | Add failed: {0} |
| CopyFailed | 复制失败：{0} | Copy failed: {0} |
| ConvertFailed | 转换失败：{0} | Convert failed: {0} |
| SelectItemToDelete | 请先选择要删除的项 | Please select an item to delete first |
| SelectItemToCopy | 请先选择要复制的项 | Please select an item to copy first |
| NoDefaultConstructor | 类型 {0} 没有无参构造函数 | Type {0} has no parameterless constructor |
| DictionaryEditorTitle | 字典编辑器 | Dictionary Editor |
| DictEntries | 字典条目 | Entries |
| DictKey | 键 (Key) | Key |
| DictValue | 值 (Value) | Value |
| DuplicateKey | 键 "{0}" 已存在，请使用唯一的键 | Key "{0}" already exists. Keys must be unique. |

---

## API 参考

### PropertyGrid 类

**命名空间：** `PropertyGridLib`

#### 依赖属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `SelectedObject` | `object` | `null` | 要编辑的目标对象 |
| `SearchText` | `string` | `""` | 搜索文本（自动过滤） |
| `ShowDescription` | `bool` | `true` | 是否显示底部描述栏 |
| `ShowSearchBar` | `bool` | `true` | 是否显示搜索栏 |
| `SelectedPropertyItem` | `IPropertyItem` | `null` | 当前选中的属性项 |
| `GroupedCategories` | `IEnumerable` | `null` | 分组后的分类列表（供绑定） |
| `IsLoading` | `bool` | `false` | 是否正在加载（切换对象时显示动画） |
| `FormulaTreeProvider` | `IFormulaTreeProvider` | `null` | 全局公式树提供者（可选） |
| `ShowTypeHint` | `bool` | `true` | 是否显示 FormulaTree 类型提示和公式绑定区类型提示 |

#### 静态成员

| 成员 | 类型 | 说明 |
|------|------|------|
| `GlobalFontFamily` | `FontFamily` | 全局字体（默认 Consolas），宿主可设置；PropertyGrid.FontFamily 变化时自动同步 |
| `GlobalFontSize` | `double` | 全局字号（默认 12），宿主可设置；PropertyGrid.FontSize 变化时自动同步 |
| `CustomEditors` | `Dictionary<Type, DataTemplate>` | 宿主自定义编辑器注册字典，AttributeType → DataTemplate |
| `RegisterEditor<TAttribute>(template)` | `static void` | 泛型注册方法，一行搞定宿主自定义编辑器 |

#### 公共方法

| 方法 | 说明 |
|------|------|
| `RefreshProperties()` | 刷新属性列表（重新反射对象） |
| `ResetSelectedToDefault()` | 重置当前选中属性到初始值 |
| `ResetAllToDefault()` | 重置所有属性到初始值 |

#### 事件

| 事件 | 说明 |
|------|------|
| `ButtonClicked`（实例） | 本网格内 `[Button]` 属性被点击时触发（参数 `PropertyButtonClickEventArgs`） |
| `GlobalButtonClicked`（静态） | 任意 PropertyGrid 实例（含集合/字典内嵌网格、弹窗网格）的 `[Button]` 被点击时触发；宿主订阅一次即可全局响应，窗口关闭时应取消订阅 |

PropertyGrid 在内部监听 `LocalizationManager.LanguageChanged` 事件，语言切换时自动刷新所有属性的本地化文本。

> **集合/字典模式行为：** 当 `SelectedObject` 为 `IList`（排除 `string`）时内联显示集合编辑器；为 `IDictionary` 时内联显示字典编辑器；两者均自动隐藏正常属性列表。

---

### ColorEditControl 类

**命名空间：** `PropertyGridLib.Controls`

颜色编辑器 UserControl，内嵌 HandyControl ColorPicker 弹窗。PropertyGrid 自动检测 `Color` / `Color?` / `SolidColorBrush` 类型，宿主无需手动使用此类。

| 成员 | 类型 | 说明 |
|------|------|------|
| `PART_ColorPreview` | `Border` | 颜色预览块（16×16），颜色随 `ColorPicker.SelectedColor` 同步 |
| `PART_ColorHexText` | `TextBlock` | HEX 文本，显示 `#RRGGBB` 或 `#AARRGGBB` |
| `PART_EditButton` | `Button` | 点击弹出 ColorPicker 弹窗 |
| `PART_ColorPicker` | `hc:ColorPicker` | 弹窗内的 ColorPicker（StaysOpen=true，Confirmed/Canceled 事件处理） |

---

### CollectionEditorControl 类

**命名空间：** `PropertyGridLib.Controls`

复杂对象列表编辑器 UserControl。左侧列表 + 右侧内嵌 PropertyGrid + 增删复制排序按钮。

| 依赖属性 | 类型 | 默认值 | 说明 |
|----------|------|--------|------|
| `Items` | `IList` | `null` | 绑定的集合数据（直接操作源集合） |

---

### SimpleCollectionEditorControl 类

**命名空间：** `PropertyGridLib.Controls`

简单类型列表编辑器 UserControl。列表 + 输入框 + 增删排序按钮。

| 依赖属性 | 类型 | 默认值 | 说明 |
|----------|------|--------|------|
| `Items` | `IList` | `null` | 绑定的集合数据（直接操作源集合） |
| `ElementType` | `Type` | `typeof(string)` | 元素类型（通常自动从集合泛型参数推断） |

---

### DictionaryEditorControl 类

**命名空间：** `PropertyGridLib.Controls`

字典编辑器 UserControl。左侧条目列表 + 右侧键/值编辑区（键、值各自可为文本框或内嵌 PropertyGrid）。直接操作传入的 `IDictionary` 实例，增删改即时写回。

| 依赖属性 | 类型 | 默认值 | 说明 |
|----------|------|--------|------|
| `Items` | `IDictionary` | `null` | 绑定的字典数据（直接操作源字典） |

> 弹窗版为 `PropertyGridLib.Dialogs.DictionaryEditorDialog`（构造传入 `IDictionary`，`ShowDialog()` 返回 true 表示写回）。

---

### FormulaBindingControl 独立使用属性

**命名空间：** `PropertyGridLib.Controls`

除 PropertyGrid 内嵌用法外，新增以下依赖属性支持独立使用：

| 依赖属性 | 类型 | 默认值 | 说明 |
|----------|------|--------|------|
| `TreeItemsSource` | `IEnumerable` | `null` | 直接设置公式树节点（`FormulaTreeNode` 集合），优先级最高 |
| `FormulaTreeProvider` | `IFormulaTreeProvider` | `null` | 设置公式树提供者，优先级次之 |
| `FormulaString` | `string` | `""` | 公式字符串（支持 TwoWay 绑定） |

---

### PropertyItem 类

**命名空间：** `PropertyGridLib.Controls`

| 属性 | 类型 | 说明 |
|------|------|------|
| `Name` | `string` | 属性名（代码中的名称） |
| `DisplayName` | `string` | 显示名称 |
| `Category` | `string` | 分类 |
| `Description` | `string` | 描述 |
| `Value` | `object` | 属性值（可读写） |
| `ValueString` | `string` | 属性值字符串表示 |
| `PropertyType` | `Type` | 属性类型 |
| `EffectivePropertyType` | `Type` | 有效类型（FormulaBound&lt;T&gt; 时返回 T；Color 类型时省略 Alpha 显示） |
| `IsReadOnly` | `bool` | 是否只读 |
| `IsBrowsable` | `bool` | 是否可浏览 |
| `IsExpandable` | `bool` | 是否可展开子属性 |
| `IsExpanded` | `bool` | 是否已展开 |
| `HasDefaultValue` | `bool` | 是否有默认值 |
| `IsDefault` | `bool` | 当前值是否等于初始值 |
| `IsModified` | `bool` | 是否已修改 |
| `CustomEditor` | `CustomEditorType` | 自定义编辑器类型（仅框架内置编辑器设置，宿主扩展为 null） |
| **`CustomEditorTemplate`** | **`DataTemplate?`** | **宿主注册的自定义编辑器模板，非 null 时 EditorTemplateSelector 优先使用** |
| **`CustomEditorAttribute`** | **`Attribute?`** | **宿主自定义编辑器对应的 Attribute 实例，模板 code-behind 可读取参数** |
| `FileFilter` | `string` | 文件过滤器 |
| `EnumValues` | `Array` | 枚举值列表 |
| `StandardValues` | `List<object>` | 下拉列表标准值 |
| `IsStandardValuesExclusive` | `bool` | 下拉列表是否排他 |
| `SliderMinimum/Maximum/Step` | `double` | 滑块参数 |
| `SliderShowNumberBox` | `bool` | 滑块旁是否显示可编辑数字框（取代数值标签） |
| `IsFormulaEnabled` | `bool` | 是否启用公式绑定 |
| `FormulaString` | `string` | 公式字符串 |
| `FormulaTreeProvider` | `IFormulaTreeProvider` | 公式树提供者 |
| `MultiSelectProvider` | `IMultiSelectProvider` | 多选列表提供者 |
| `ChildProperties` | `List<PropertyItem>` | 子属性列表 |
| `ResetCommand` | `ICommand` | 重置命令 |
| `BrowseFileCommand` | `ICommand` | 浏览文件命令 |
| `BrowseDirectoryCommand` | `ICommand` | 浏览目录命令 |
| `EditCollectionCommand` | `ICommand` | 编辑集合命令 |
| `ButtonText` | `string` | 按钮文本（`[Button]`；为空时回退到 DisplayName） |
| `ButtonClickCommand` | `ICommand` | 按钮点击命令（`[Button]`） |

#### CustomEditorType 枚举

| 值 | 说明 |
|----|------|
| `None` | 无自定义编辑器（按类型自动选择） |
| `FilePath` | 文件路径选择器 |
| `DirectoryPath` | 目录路径选择器 |
| `Collection` | 集合编辑器 |
| `Expandable` | 可展开对象 |
| `Slider` | 数值滑块 |
| `DropDown` | TypeConverter 下拉列表 |
| `Formula` | 公式绑定 |
| `MultiSelect` | 多选列表 |
| `Button` | 命令按钮（`[Button]`） |
| `Dictionary` | 字典编辑器（`IDictionary`） |
| `Color` | 颜色编辑器（`Color` / `SolidColorBrush`） |

---

### FormulaTreeNode 类

**命名空间：** `PropertyGridLib.Controls`

| 属性 | 类型 | 说明 |
|------|------|------|
| `Header` | `string` | 节点显示文本 |
| `Formula` | `string` | 选中后填入的公式字符串（仅叶子节点需要设置） |
| `Children` | `List<FormulaTreeNode>` | 子节点列表 |
| **`FormulaType`** | **`Type?`** | **叶子节点的数值类型（如 `typeof(int)`），用于 TreeView 弹窗显示类型提示** |
| **`TypeHint`** | **`string`** | **自动从 `FormulaType` 生成的类型提示（如 `(int)`、`(string)`），宿主也可直接覆盖** |

---

### LocalizationManager 类

**命名空间：** `PropertyGridLib.Localization`

| 成员 | 类型 | 说明 |
|------|------|------|
| `CurrentLanguage` | `Language` | 当前语言（默认 `ZhCN`） |
| `LanguageChanged` | `event EventHandler` | 语言切换事件 |
| `GetString(key)` | `string` | 获取本地化字符串 |
| `GetString(key, args)` | `string` | 获取本地化字符串（带格式化参数） |

---

## 依赖项

| 依赖 | 版本 | 说明 |
|------|------|------|
| .NET Framework | 4.8+ | 支持的目标框架之一 |
| .NET | 6.0/7.0/8.0/10.0-windows | 支持的现代框架 |
| HandyControl | 3.5.1 | 官方 HandyControl UI 控件库 |
| System.Text.Json | 6.0.0+ | 仅 .NET Framework 4.8 需要（.NET 6+ 内置） |

**版本兼容性：**
- System.Text.Json 向上兼容到 16.0+
- 你的项目可以使用更高版本的 System.Text.Json，不会产生冲突
- .NET 6/7/8 使用内置版本，无需额外引用

---

## 更新日志

### v1.7.0（2026-10-25）

#### ✨ 新增

- **三个编辑器可独立使用**：集合、字典、事件编辑器不再依赖 `PropertyGrid`，宿主直接给 `Items` / `Dictionary` / `HandlerName` 赋值即可嵌入任意容器。

  - `CollectionEditorControl.Compact` / `DictionaryEditorControl.Compact`：紧凑形态只占一行（值框 + `...`），点 `...` 打开编辑弹窗，关闭后刷新摘要；默认 `False` 保持完整内联编辑面板。
  - `EventEditorControl` 独立用法：`HandlerName` / `IsBound` / `IsReadOnly` / `Placeholder` 四个依赖属性；请求编辑经 `RequestEditEvent` 路由事件抛出，参数携带 `EventItem`（独立用法下为 null，宿主可从发送方读 `HandlerName`）。
  - `PropertyGrid.ShowHeader`：顶部标题区开关；`PropertyGridPro.ShowHeader` 同名同语义。
  - `PropertyGridPro.ShowDescription`：底部描述栏开关。
  - `PropertyCategory.ShowGroupSeparator`：分类头顶部画主题色细线，与 Section 布局的行分隔线同一套观感。

#### 🔧 改进

- **切换语言不乱序**：属性与分类的排序改用特性原文作基准（`SortCategory` / `SortDisplayName`），本地化只换显示不换顺序。
- **Pro 滚动条抖动修复**：`PropertyGridPro.EnableVirtualization` 默认改为关闭。Pro 行高差异大（图片预览行约 200、纯文字行约 90），平均行高估算会让滚动条忽大忽小、拖到底还留一大片空白。行数少于 `VirtualizationRowThreshold`（150）时无论开关都不虚拟化；超过阈值且宿主显式开启才走虚拟化。
- 事件列表虚拟化按行数自适应，修掉滚到底残留大片空白。
- 基础网格公式绑定默认显示目标类型提示（`(int)` 之类），嫌挤可显式关闭。
- 描述栏隐藏时整行压 0，不在底部留空条。

#### 🐛 缺陷修复

- 普通版 `ShowDescription` 先关再开后描述栏不显示（开启时补一次选中项重算）。
- 分类头分隔线在窄面板下压到分类标题。

### v1.6.0（2026-10-15）

#### 🐛 缺陷修复

- **可展开复杂对象值区显示完整 CLR 类型名**：嵌套配置类（如 `Ext.FlowWork.Settings.AutoBackupOptions`）未重写 `ToString()` 时，对象行右侧只读值区直接显示默认的 `命名空间.类名` 全名，过长且对业务用户无意义。`PropertyItem.ValueString` 末尾回退改为：重写了 `ToString()` 的输出原样保留 → 类型级 `[DisplayName]` 业务名 → 友好类型名（复用 `GetFriendlyTypeName`：去命名空间、展开泛型参数、可空加 `?`）。枚举成员名（`System.Enum.ToString`）与未重写 ToString 的结构体（`ValueType.ToString`）均正确分流，集合计数/颜色色值等特判不受影响。

- **PropertyGridPro 平铺行宽窗口布局修正**：恢复「标题列（左）+ 编辑器列（右、固定 EditorWidth 贴卡片右缘）」两栏结构——窗口拉宽时新增空间落在两栏之间；标题 StackPanel 显式 `HorizontalAlignment=Left`，宽窗口下标题与卡片左缘之间只留卡片自身 Padding，不再在标题左侧留出大片空隙。

- **多层嵌套对象第二层及更深层无法展开（关键修复）**：网格（`PropertyGrid` 与 `PropertyGridPro`）刷新时只对**顶层**属性项挂钩 `PropertyChanged`，而展开后惰性生成的子属性项（`PropertyItem.LoadChildProperties` 创建）从未被订阅。子项自身 `IsExpanded` 变化无人接收，扁平行集合（`BuildFlatRows`）不会重建 —— 现象为「第一层可展开，第二层及更深层点箭头无任何反应」。修复：`PropertyItem.OnPropertyChanged` 在 `IsExpanded` 变更时**逐级冒泡到父项**（顶层项 `Parent == null`，链条自然终止），由网格收到顶层项通知后统一重建一次行集合。任意嵌套深度（受 `maxDepth` 限制，默认 5）均可逐层展开；经典网格与平铺面板共用同一冒泡路径，一次修复同时生效。

- **double/float 数值显示长尾数（浮点噪声）**：`ValueString` 原先对数值走默认 `ToString()`——给的是「最短往返表示」，会把二进制舍入噪声原样吐出：滑块按 0.1 步进到 1.2 时 double 实为 `1.2000000000000002`，数值框与滑块标签便显示成一长串小数（框宽不足还会被裁成 `1.200000000000`）；`0.1+0.2` 则显示 `0.30000000000000004`。修复：`ValueString` 对 `double` 用 **G15**、`float` 用 **G7** 有效数字格式输出（`0.1+0.2 → 0.3`、`12*0.1 → 1.2`，G 格式自带去尾零），结果落到科学计数法（如 `1.5E-05`）时再改用定点格式，避免面板出现 E 记数写法。**只改显示文本，底层存储与序列化仍是原始双精度值**（实测属性值保持 `1.2000000000000002` 不变）。`decimal` 无二进制噪声，维持原样输出。

  > 一并说明：HC `NumericUpDown` 的 `ValueFormat` 属性在本框架依赖的 HandyControl 3.5.1 中实测被忽略、`DecimalPlaces` 只能固定小数位，均无法根治；统一在 `ValueString` 层格式化后，**只读值区、滑块数值框、普通数值框、数值标签四条显示路径同时生效**。

- **公式绑定「打开即已绑定状态」**：公式已有内容时，绑定区默认直接进入已绑定态，不再停在普通输入/原始值上等用户手动点开。
  - `FormulaSwitchControl`：`FormulaString` 非空时默认 `IsFormulaMode = true`（打开即公式模式）；公式为空则保持普通输入。只在首次「无内容 → 有内容」补默认值，用户手动切回普通输入后不会被反复拉回。
  - `PropertyItem`（网格属性行）：`[FormulaEditor]` / `FormulaBound<T>` 属性若已带公式，`IsFormulaEditorOpen` 默认 `true`，公式条默认展开显示在行内（不再需要点绑定按钮）；空公式保持默认收起。
  两处都只在「公式确有内容」时改变默认态，原有手动切换行为不变。

- **PropertyGridPro 公式条被卡片撑高**：Pro 卡片行高度由左侧「属性名 + 说明」决定（说明折成多行时卡片明显比编辑器高），而公式覆盖条原先没有纵向约束，会被拉满整张卡片，公式输入框随之变成一块又高又空的圆角白块。修复：条带加 `VerticalAlignment=Center` + 固定高度 `30`（与其他行编辑器统一高度，实测 30.4px）。覆盖层背景仍铺满整格、原值编辑器依旧被盖住，只是条带本身回到正常编辑器高度。Classic 两套行模板行高固定为 34，本就不受影响，未改动。

- **编辑器列表显示名不随属性更新（集合/字典编辑器刷新缺陷）**：集合编辑器弹窗与字典编辑器的左侧列表，行文本是**加载时按 `ToString()` 算好的一次性快照**（`ListBoxItemData.DisplayText` / `DictEntry.KeyDisplay·ValueDisplay`）。在右侧内嵌属性页里改了条目属性（改名称、换选项），改的是**同一个条目对象**（引用不变），列表行没有任何刷新路径 —— 于是显示名一直停在旧值。
  - 修复：`ListBoxItemData` 实现 `INotifyPropertyChanged`（`DisplayText` 变更只通知该行，不重建 `ItemsSource`、不丢选中项），并新增 `ListBoxItemData.RefreshDisplayTexts(IEnumerable)`；`DictEntry` 新增 `RaiseDisplayChanged()` 与 `DictEntry.RefreshDisplayTexts(IEnumerable)` 静态方法。
  - 四个宿主**全部**接入：`CollectionEditorDialog`（弹窗）、`CollectionEditorControl`（内联）、`DictionaryEditorDialog`（弹窗）、`DictionaryEditorControl`（内联），在各自内嵌属性页的 `PropertyValueChanged` 上重算显示文本。
  - 只刷显示层：**不动数据、不重建列表、不影响选中项**，「确定才写回、取消丢弃」的既有语义完全不变。
  - 实测（端到端探针）：集合编辑器改「预置位名称」`停车场1111 (45°,60°) → 停车场 (45°,60°)`、改「水平角度」`→ 停车场 (30°,60°)`，未编辑的行不受影响；字典编辑器改复杂值内部属性 `大门1111 (180°,90°) → 大门 (180°,90°)`，底层对象值同步正确。

- **去除行内「修改红点」**：紧凑布局（`TitlePlacement=Left`）的属性行原先在属性名旁画了一个 6px 的红色圆点表示「已修改」，与行尾的 `R` 复位按钮是同一个标识的冗余重复。按宿主反馈**移除红点**，保留 `R` 按钮及其「未修改时隐藏（保留占位、不抖动）」的既有行为。其余行模板（Top / Pro）本就没有红点，未改动。

- **事件页（EventPicker）与属性页对齐：分类可折叠 + 底部描述栏**：
  - **分类折叠**：`EventCategory` 升级为与 `PropertyCategory` 同构（新增 `IsExpanded` + `INotifyPropertyChanged`）；平铺行的分类头由纯标签改为 `Expander`（`IsExpanded` TwoWay 绑定），折叠时该类事件行不再进入平铺列表（**不退回嵌套 Expander**，仍保持「压平 + 虚拟化」，避免历史上有过的虚拟化估算抖动）。折叠状态按分类名跨重建保留 —— 搜索输入每敲一键都会重建分类对象，若不记住则折叠会被反复弹开。
  - **底部描述栏**：新增 `EventPicker.SelectedEvent`（点击事件行时更新），控件底部出现与属性页同款排布的描述栏（标题 + 说明）；`EventItem` 新增 `HasDescription`，**只有选中事件确有说明时才出现**（属性页即使无说明也会留一条空栏，事件页不做这种占位）。
  - 实测（探针）：`TitlePlacement=Left` 行内 `Ellipse` 数量 0；事件页初始 6 行 → 折叠 Behavior 后 4 行 → 走一次完整重建（等价搜索/换对象）仍为 4 行且展开状态保持 → 重新展开回 6 行；选中带说明的 `Click` 时描述栏可见、选中无说明的 `MouseDown` 时隐藏。

  ![EventPicker 事件页（分类折叠 + 底部描述栏）](https://raw.githubusercontent.com/1wangshuo/PropertyGridDemo/main/skill/images/event-picker-light.png)

- **分类头底色改为面板色**：分类头用的是 HandyControl 的 `Expander`，其默认 `Background` 取 `SecondaryRegionBrush`（浅色皮肤 `#EEEEEE`），于是在浅色主题下渲染成一条比面板更深的灰带、深色主题下也是一条突兀的深灰横带。修复：属性页与事件页的分类头 `Expander` 都显式 `Background="{DynamicResource RegionBrush}"` —— 浅色皮肤下就是 `#FFFFFF`（近白面板色），深色皮肤下是正常深色面板，**仍然完全跟随主题、不写死任何色值**。实测分类头底色由 `#FFEEEEEE` 变为 `#FFFFFFFF`。

- **「重置所有属性」连续点击闪退（关键崩溃修复）**：反复重置（或任何密集的值变更 + 紧随布局）时，UI 线程在布局里抛 `COMException 0x8000FFFF (E_UNEXPECTED)` —— 调用栈 `SolidColorBrush.ReleaseOnChannelCore → Visual.DisconnectAttachedResource → UIElement.RenderClose → Arrange → ContextLayoutManager.UpdateLayout`，进程直接退出（异常码 `0xE0434352`，Windows 事件日志 Application Error）。根因定位在**颜色编辑器行内的色块**：模板把该 `Border.Background` 绑到 `{Binding Value, Converter={StaticResource ColorToBrush}}`，而 `ColorToBrushConverter` 每次求值都 `new SolidColorBrush(Color)` 并冻结；属性值一变就换新画刷实例，旧实例在紧随的布局里被 WPF 从组合通道释放（`SolidColorBrush.ReleaseOnChannelCore`），连点重置时即命中释放失败。
  - 修复一：`ColorToBrushConverter` 改为**按颜色缓存冻结画刷**（`ConcurrentDictionary<Color, SolidColorBrush>` + `GetFrozenBrush`），不再一次一 new；`ColorEditControl` 的 `ValueToBrush` / `Confirmed` 也改走同一缓存。
  - 修复二（决定性）：`ColorEditControl` 新增**控件独占、不冻结**的画刷 `SwatchBrush`，模板色块 `Background` 改绑它（`{Binding SwatchBrush, RelativeSource={RelativeSource TemplatedParent}}`）；值变化只就地改该实例的 `Color`，**绑定值从头到尾是同一个实例**，不再出现「换画刷 → 旧实例被释放」的循环。
  - 实测（端到端探针，先改 46 个属性制造真实值变更，再连点重置 + 每轮泵布局）：修复前 `page`（虚拟化开）**第 1 轮就崩**；修复后 `page` 60 轮、`novirt` 30 轮、`pro` 30 轮**全部通过、零异常**。**A/B 对照**：只把色块绑定改回旧的转换器写法，同一调用栈的崩溃立刻复现 —— 修复点单一、因果明确。
  - 显示无回归（探针断言）：值变化前后色块 `Background` 为**同一画刷实例**、`Color` 正确跟随（`#FFFF0000 → #FF00FF00`），深浅主题下渲染正常。

#### 🎯 平台支持

- 随 NuGet 包分发 `net10.0-windows` 程序集（多目标 `net48;netcoreapp3.0;netcoreapp3.1;net6.0-windows;net7.0-windows;net8.0-windows;net10.0-windows`）。

#### 🔄 向后兼容性

- 仅修正默认显示文本，不改变属性读写、编辑器选择与序列化行为；重写了 `ToString()` 的既有显示零变化。

### v1.5.0（2026-10-02）

#### 🐛 缺陷修复

- **FormulaSwitchControl 公式模式下双输入框重叠**：进入公式模式时外层普通输入框（`PART_TextBox`）未收起，与 `FormulaBindingControl` 内部输入框叠在同一格，出现两个文本框重叠、🔗/✕ 按钮挤压。根因是隐藏逻辑挂在 `ControlTemplate.Triggers` 的 `DataTrigger` 上未生效；改为在 `PART_TextBox` 上直接绑定 `Visibility`（`IsFormulaMode` 经 `BoolToVisibilityConverter` + `Invert` 参数），与公式绑定条互斥显隐。

#### 🔄 向后兼容性

- ✅ 无 API 变更；仅 `FormulaSwitchControl` 模板内部显隐逻辑修正，宿主用法不变

### v1.4.1（2026-09-28）

#### ✨ 新增功能

- **🈶 属性/事件显示名翻译表（宿主注册）**：反射场景（如 WPF 设计器把控件实例直接交给 `SelectedObject`）没有 `[DisplayName]` 可用，属性名只能显示 `PropertyInfo.Name` 原文。新增 `LocalizationManager.RegisterPropertyName(name, displayName)` / `RegisterPropertyNames(map)` / `ClearPropertyNames()` / `TryGetPropertyName(name)`：宿主把「CLR 名 → 显示名」词条注册进来后，`PropertyItem` 与 `EventItemFactory.FromEventInfo` 在无 `[DisplayName]` 时自动查表（取词链：`[DisplayName]` 特性 → 宿主翻译表 → 英文原名）；仅中文环境（`zh*`）命中，其余语言回退原名，语言切换随 `UpdateLocalization()` 自动生效。CodeForge.DesignerKit 据此内置了约 300 条 WPF 常用属性/事件中文词典（WpfDesignerNames，XamlFormController 构造时自动注册）

### v1.4.0（2026-09-24）

#### ✨ 新增功能

- **📐 Small 紧凑布局（`TitlePlacement` 新属性）**：属性行标题排布可在「编辑器上方（`Top`，默认）」与「编辑器左侧（`Left`，Small 模式）」之间切换。`Left` 时标题与值编辑器平齐一行，对齐 HandyControl 表单左标题风格，行高压缩近一半；**R 复位按钮保留在编辑器右侧**（未修改时隐藏占位，编辑时布局不抖动），值被修改时标题右侧显示红点提示；公式绑定区与子属性列表因宽度原因**仍另起一行**
  - 实现方式：由 `PropertyItem` 隐式 `DataTemplate` 内的 `DataTrigger` 驱动（绑定祖先 `PropertyGrid.TitlePlacement`），顶层扁平列表、分类内列表、各级子属性列表**一处切换全局生效**，无需逐个 `ItemsControl` 设置 `ItemTemplate`
  - 用法：`<pg:PropertyGrid TitlePlacement="Left"/>` 或代码 `grid.TitlePlacement = PropertyGridTitlePlacement.Left;`
- **🅰️ 标题与分类头字体粗细（`TitleFontWeight` / `CategoryFontWeight`）**：新增两个 `FontWeight` 类型依赖属性，分别控制属性行标题与分类头（`Expander` 标题）的粗细，默认值均为 `Bold`（即截图里「标题在左」或「标题在上」的 `1.设备名称` / `I.基本信息` 都会加粗）。如果宿主希望更纤细，可设为 `Normal` / `Light`
  - 用法：`<pg:PropertyGrid TitleFontWeight="Normal" CategoryFontWeight="Bold"/>`
  - 说明：该属性通过 XAML 绑定直接生效，切换时无需调用 `RefreshProperties`；Top / Left 两种行模板均已绑定
- **⚡ 属性列表 UI 虚拟化（性能重点）**：属性列表由「类别 Expander → 内层 ItemsControl」的**两层嵌套压平为单层列表**（类别头 + 其下的属性行同级排列），并开启 UI 虚拟化 —— 只生成视口内的十几行并复用行容器，而不是每次刷新都全量销毁重建上百行（80 属性 + 60 事件，每行含 NumericUpDown / ComboBox / ColorPicker 等重编辑器）。实测**切换选中 → 属性页重建 206ms → 8ms**，重复刷新的**托管堆增长 44.5MB → 14.6MB**。属性仍全部可见，不是靠默认折叠换来的，API 与用法不变
  - 关键坑（已在本次解决）：`ScrollViewer` 必须放进 `ItemsControl` 的模板内、`Content` 用 `ItemsPresenter`，且 `CanContentScroll="True"`。外置 `ScrollViewer` 时它的 Content 是 `ItemsControl` 而非 `IScrollInfo`，找不到内部的 `VirtualizingStackPanel`，会退化成物理滚动、全量生成行 —— 第一版就是这么改的，压测证实毫无效果（209ms，与未开一致）
  - 公式绑定区与子属性列表由 `Visibility` 折叠改为**按需创建**（原先每行都实例化一个 `FormulaBindingControl`，80 行白白创建 80 个）
  - 新增 `EnableVirtualization` 开关（默认 `true`）。若宿主自定义行模板出现「滚动后行内值串行」，设为 `false` 即回到 1.3.4 的全量生成行为
- **🔌 事件页能力**：`ShowEventsPage` 开关 + `EventPicker` 事件筛选选择器 + `EventItem` 模型 + `EventEditorControl` 行内编辑器；**默认关闭**，不开时控件行为与旧版完全一致。开启后顶部出现「属性 | 事件」切换，事件行双击或点 ⚡ 触发 `EventEditRequested`，便于设计器宿主把事件绑定与代码编辑器（如 CodeForge）打通

#### 🐛 缺陷修复

- **`Value` / `ValueString` 增加相等性检查 —— 消除「无用户操作却触发属性变更事件」**：TwoWay 绑定在「绑定建立 / 虚拟化容器复用」时会把控件的当前值推回源。此前两个 setter 都没有相等性判断，即使推回的值与当前值**完全相同**（如 `"False"` vs `False`）也会写回宿主对象并触发 `ValueChanged` → `PropertyValueChanged`。压测实测：**30 秒内误触发上百次**，涉及 bool 属性的开关编辑器与 `Visibility` / `HorizontalAlignment` / `FlowDirection` 等枚举属性；设计器宿主会据此把同一个值反复写回 XAML（污染撤销栈、无谓开销）。修复后**误触发降为 0 次**，同时保留 472 次/轮的绑定推回调用被正确短路
  - 澄清一个容易误判的点：**开关（ToggleSwitch）的切换动画不会触发任何写入**。动画是纯视觉层（滑块位移），不改变 `IsChecked` 逻辑值。真正的写入来源是绑定推回，两者不可混淆
- **`SelectedObject` 置 null / 传入已销毁对象时崩溃**：防抖路径会拿陈旧的选中项去访问已释放的实例，抛 `NullReferenceException`
- **加载指示动画**：`LoadingCircle` 改为静态文本。动画时钟在「快速显隐」的元素上持续 tick，是宿主侧 CLR 闪退的诱因之一，静态化后无时钟、无竞态

#### 🔄 向后兼容性

- ✅ 公共 API 无破坏性变更；`EnableVirtualization` / `ShowEventsPage` / `TitlePlacement` 均为新增开关，默认值（`true` / `false` / `Top`）下渲染结果与旧版完全一致
- ⚠️ 虚拟化开启后行容器会被回收复用（`Recycling`）。使用**自定义行模板**且模板内含非绑定状态（如在 `Loaded` 里缓存对象引用）的宿主，请验证滚动后的取值；有异常时把 `EnableVirtualization` 设为 `false` 即可回退

### v1.3.0（2026-09-15）

#### ✨ 新增功能
- **🧩 PropertyGridPro 平铺参数面板（新控件）**：新增 `PropertyGridLib.PropertyGridPro`，纵向平铺布局 + 卡片式属性行，面向「参数面板 / 属性面板」场景。提供 `Header` / `HeaderDescription` / `SourceText` 头部区、`ShowHeader` / `ShowSearchBar` / `ShowDescription` 开关、`EditorWidth` 统一编辑器宽度、`RowTemplate` 整体替换行模板；属性搜索附匹配计数与空结果提示；提供 `RefreshProperties()` / `ResetAllToDefault()` / `ResetSelectedToDefault()` / `SetSubPropertiesExpanded(bool)` 方法。与 `PropertyGrid` 共用同一套特性体系、编辑器模板、本地化与主题资源，模型代码无需改动即可互换
- **🃏 卡片式属性行布局**：属性行改为卡片承载（圆角 7、行间距、分类头独立成卡片），卡片底色取主题次级区域画刷（`SecondaryRegionBrush`），深浅主题下层次一致
- **🌓 主题色跟随（去除硬编码）**：面板模板中硬编码的深色值全部替换为 `SkinDark` / `ThemeHelper` 等主题资源键，卡片背景、边框、标题与描述文字、分类头、嵌套引导线均随主题切换；资源键缺失时自动回退到 HandyControl 默认画刷
- **🪜 无限嵌套样式统一**：复杂对象子属性以卡片逐层铺开，按层级增加缩进与引导线（层级不限）；统一各层级的间距、圆角与引导线样式，深层嵌套不再视觉走形
- **🔗 公式绑定接入 Pro 面板**：标注 `[FormulaEditor]` 的属性在 `PropertyGridPro` 中同样渲染公式绑定区（值编辑器 + 链接图标 + 公式文本框 + 树选择器 Popup），并支持全局 `FormulaTreeProvider` 与类型提示
- **📐 复位按钮常驻占位**：复位按钮可见性由 `Collapsed` 改为 `Hidden`，未悬停时仍保留占位，消除「鼠标移入/移出导致同行编辑器左右跳动」的问题（实测位移 0px）；基础控件与 Pro 面板一并生效
- **🔀 ToggleSwitchAttribute — 布尔开关特性**：新增 `PropertyGridLib.Attributes.ToggleSwitchAttribute`，`bool` 属性渲染为滑块式开关（圆角轨道 + 圆形滑块，基于 HandyControl `ToggleButtonSwitch` 样式）+ 可选的开/关状态文字；支持 `[ToggleSwitch]`（默认「开 / 关」）、`[ToggleSwitch("已启用","已禁用")]`、`OnText` / `OffText` 命名参数（传 `null` 隐藏文字）；仅对 `bool` 生效，标注在其它类型上被忽略；基类与 Pro 面板通过同一 `EditorSelector` 复用同一模板
- **📚 技能文档**：新增多页使用文档 `skill/SKILL.md` 及 8 个分页（快速开始 / 特性清单 / 编辑器类型 / 主题与多语言 / 公式绑定 / PropertyGridPro / 示例模型 / FAQ），配套截图统一存放于演示仓库的 `PropertyGridDemo/skill/images/`

#### 🐛 缺陷修复
- **复位按钮导致布局抖动**：隐藏态使用 `Collapsed` 会挤动同行值编辑器，现改用 `Hidden`（新增 `BoolToHiddenVisibilityConverter`）
- **浅色主题下 Pro 面板发暗**：面板模板中的硬编码深色画刷改为主题资源键，浅色/深色双向切换均正常
- **Pro 面板嵌套样式不一致**：统一各级子属性的间距、圆角与引导线

#### 📦 新增公共类型
- `PropertyGridLib.PropertyGridPro` — 平铺参数面板控件
- `PropertyGridLib.Attributes.ToggleSwitchAttribute` — 布尔开关特性（`OnText` / `OffText`）
- `PropertyGridLib.Converters.BoolToHiddenVisibilityConverter` — 隐藏（保留占位）可见性转换器
- `PropertyItem.ToggleOnText` / `ToggleOffText` / `ToggleStateText` / `ToggleStateTextVisibility` — 开关状态文字相关成员
- 编辑器模板属性：`EditorTemplateSelector.BooleanTemplate`（默认 bool 开关）、`EditorTemplateSelector.ToggleSwitchTemplate`（带状态文字的开关）

#### 🔄 其它变更
- 演示程序移除旧模型与「实时值」区块，Pro 演示窗口改用与基类一致的 `SampleObject`
- 版本号字段（`Version` / `AssemblyVersion` / `FileVersion` / `InformationalVersion`）同步为 `1.3.0`，`Description` 与 `PackageTags` 补充 PropertyGridPro / ToggleSwitch

#### 🔄 向后兼容性
- ✅ 现有公共 API 与行为全部保留，1.3.0 均为新增或视觉优化
- ⚠️ 未标注 `[ToggleSwitch]` 的 `bool` 属性外观为滑块开关（无状态文字），如宿主此前依赖自定义 bool 模板请确认未被覆盖
- ⚠️ 复位按钮的隐藏态由 `Collapsed` 改为 `Hidden`，若宿主自行覆写了行模板中的可见性转换器需同步调整

### v1.2.0（2026-09-07）

#### ✨ 新增功能
- **🔌 宿主自定义编辑器扩展机制**：`PropertyGrid.CustomEditors` 静态字典 + `RegisterEditor<TAttr>(template)` 泛型方法；宿主只需定义 Attribute + DataTemplate + 一行注册，PropertyGrid 即可渲染自定义编辑器；框架对此 Attribute 一无所知。新增 `PropertyItem.CustomEditorTemplate` / `CustomEditorAttribute` 属性，`EditorTemplateSelector` 优先返回宿主模板，宿主甚至可覆盖框架内置编辑器
- **🎨 颜色编辑器**：自动识别 `Color` / `Color?` / `SolidColorBrush` 类型（内置 `IsColorType` 检测在 `CanExpand` 之前拦截，防止 Color struct 被误判为可展开对象）；显示为颜色预览块（16×16 + `#RRGGBB` HEX 文本）+ "..." 按钮（紧凑居左，与 FilePath/Dictionary 编辑器风格统一）；点击弹出 HandyControl `ColorPicker`（`StaysOpen=true` + 手动 `Window.PreviewMouseDown` + `IsInVisualTree` 外部点击检测，支持拖动 Slider 不丢失焦点；"确定"才写入值并关闭，"取消"不改值）；HEX 显示：不透明时省略 Alpha 前缀
- **📁 字典嵌套字典/列表自动弹窗**：字典值为集合（`IDictionary` / `IList`）时不再内联显示内嵌 PropertyGrid，改为显示 "N 项 + ..." 按钮，点击弹出二级 DictionaryEditorDialog / SimpleCollectionEditorDialog（模态覆盖父级，UI 不会无限拉长）；字典值为复杂对象时仍内嵌 PropertyGrid 就地编辑（三种模式互斥）
- **🌳 FormulaTree 类型提示**：`FormulaTreeNode` 新增 `FormulaType` / `TypeHint` 属性，TreeView 弹窗叶子节点右侧显示淡蓝色斜体类型名（如 `(int)`、`(DateTime)`）；PropertyGrid 新增 `ShowTypeHint` 依赖属性（默认 true）控制是否显示；公式绑定区同样显示类型提示
- **🔤 全局字体动态同步**：`PropertyGrid.FontSize` / `FontFamily` 通过 `DependencyPropertyDescriptor` 实时监听，变化时自动更新 `GlobalFontSize` / `GlobalFontFamily`；所有 Dialog（字典、集合、文件夹、颜色）打开时读取最新全局值，FontSize/FontFamily 跟随宿主变化
- **📁 FolderBrowserDialog 主题修复**：所有文字控件显式指定 `Foreground="{DynamicResource PrimaryTextBrush}"` + `Background="{DynamicResource RegionBrush}"`，深色主题下不再出现白字黑底问题；TreeView/TreeViewItem 选中/悬停触发器适配深浅色主题
- **🎨 ColorPicker 弹窗图标适配**：FolderBrowserDialog 文件夹图标从 emoji 改为 Material Design 风格 Path 图标（`Fill="{DynamicResource PrimaryBrush}"`），选中时 `Foreground` 自动变白

#### 🐛 缺陷修复
- **HandyControl 主题切换失效**：原 `SetAppSkin` 用 `is HandyControl.Themes.Theme` 匹配不到 App.xaml 里通过 URI 加载的 Theme.xaml（那是 ResourceDictionary，不是 Theme 实例），导致替换 MergedDictionaries 时 idx=-1 每次 Add 新 Theme 到末尾，旧的还留着，第二次切换资源冲突——现改为直接改 `Theme.Skin` 属性（HC 官方推荐方式），白→黑→白双向切换均正常
- **颜色编辑器选完值不生效**：原 XAML 双向绑定 `ValueString` 用 `ColorToBrushConverter`，但 WPF 调 `ConvertBack` 时 `targetType=object`（因为 `PropertyItem.Value` 是 object），类型检查全挂；现改 code-behind，在 `Confirmed` 事件里根据 `EffectivePropertyType` 做正确类型转换（`Color`/`SolidColorBrush`/`Color?`）
- **ColorPicker 弹窗 Slider 拖动时消失**：`StaysOpen=false` 时 HC ColorPicker 内部 Slider 的 MouseCapture 状态变化导致 Popup 误判"鼠标在外部"——现改为 `StaysOpen=true` + 手动 `Window.PreviewMouseDown` + `IsInVisualTree` 检测外部点击

#### 📦 新增公共类型
- `PropertyGridLib.Controls.ColorEditControl` — 颜色编辑器 UserControl（弹出 HC ColorPicker）
- `PropertyGridLib.Controls.ColorToBrushConverter` — Color ↔ SolidColorBrush 双向转换器
- `PropertyGridLib.Controls.FormulaTreeNode.FormulaType` / `TypeHint` — FormulaTree 类型提示

#### 🔄 框架侧删除
- ❌ `PropertyGridLib.Attributes.UnitEditorAttribute`（框架内置的单位编辑器特性）
- ❌ `CustomEditorType.Unit` 枚举值
- ❌ `PropertyItem.UnitOptions` / `SelectedUnit` / `UnitDisplayValue` 属性
- ❌ Generic.xaml 里的 UnitTemplate
- **原因**：宿主扩展机制 `RegisterEditor<TAttr>(template)` 可完全替代，框架内置编辑器与宿主扩展功能重复

#### 🔄 向后兼容性
- ✅ 现有公共 API 全部保留（新增成员不影响旧代码）
- ⚠️ `PropertyGrid.FontSize` / `FontFamily` 现在会自动同步到全局值（之前只在 OnApplyTemplate 同步一次），若宿主依赖"全局值独立于实例值"的行为需注意
- ⚠️ 字典编辑器嵌套值的渲染模式变化：集合类型值从"内嵌 PropertyGrid"改为"弹窗"，UI 更紧凑但编辑交互从就地改为弹窗；复杂对象值仍保持内嵌模式

### v1.1.2（2026-09-04）

#### ✨ 新增功能
- **命令按钮编辑器 `[Button]`**：属性标记 `[Button("文本", nameof(方法))]` 后渲染为按钮；点击时①反射调用宿主对象上的 `Action` 方法（支持无参或单参），②触发 `PropertyItem.ButtonClicked` 与 `PropertyGrid.ButtonClicked` 事件；`Text` 为空时回退到 `DisplayName`（随多语言切换）。按钮默认始终可点，仅显式 `[ReadOnly(true)]` 时禁用
- **全局按钮事件 `PropertyGrid.GlobalButtonClicked`（静态）**：任何 PropertyGrid 实例（包括集合/字典编辑器内嵌的网格、弹窗内的网格）触发 `[Button]` 都会引发，宿主订阅一次即可跨窗口/跨层级统一响应
- **字典编辑器（IDictionary）**：自动识别 `IDictionary` / `IDictionary<,>` 属性（无需特性，`[CollectionEditor]` 标在字典上也会路由到字典编辑器）；提供弹窗 `DictionaryEditorDialog` 与内联 `DictionaryEditorControl`（当 `SelectedObject` 直接是字典时自动内联）；键、值各自支持简单类型（文本框 + 类型转换）或复杂对象（内嵌 PropertyGrid，可继续向下嵌套集合/字典）；键值均为对象时左右各占 50%；支持增/删/改、重复键校验、实时写回原字典
- **多框架**：新增 .NET Core 3.0 / 3.1 目标（实际目标：net48 / netcoreapp3.0 / netcoreapp3.1 / net6.0-windows / net7.0-windows / net8.0-windows）

#### 🐛 缺陷修复
- **字典 "..." 按钮无响应**：此前字典被 `ICollection` 识别为集合，但 `EditCollection` 仅处理 `IList`，导致点击编辑按钮无反应——现按 `IDictionary` 路由到字典编辑器
- **嵌套网格中按钮失效**：集合/字典编辑器内嵌 PropertyGrid 的按钮点击无法上抵宿主、且 Action 修改不刷新——现通过全局事件 + 点击后网格自刷新解决
- **字典编辑器描述栏白条**：内嵌网格 `ShowDescription` 不一致导致底部描述栏折叠后露出 80px 白色空行——现键/值网格统一显示描述栏

#### 📦 新增公共类型
- `PropertyGridLib.Attributes.ButtonAttribute`
- `PropertyGridLib.Controls.PropertyButtonClickEventArgs`
- `PropertyGridLib.Controls.DictionaryEditorControl`（内联字典编辑器 UserControl，`Items` 为 `IDictionary`）
- `PropertyGridLib.Dialogs.DictionaryEditorDialog`
- `CustomEditorType` 新增枚举值：`Button`、`Dictionary`

#### 🔄 向后兼容性
- ✅ 现有公共 API 与行为全部保留，仅新增成员
- ⚠️ `IPropertyItem` 接口新增了 `ButtonText` / `ButtonClickCommand` / `ButtonClicked`（库内仅 `PropertyItem` 实现该接口；若宿主自行实现过该接口，需补充这几个成员）
- ⚠️ .NET Core 3.0/3.1 目标仍为 **Windows 专用**（WPF 不支持 Linux/macOS）；因其 TFM 无 `-windows` 后缀，跨平台项目误装后运行时才会报错，请注意

### v1.1.1（2026-09-01）

#### ✨ 新增功能
- **PropertyGrid 集合模式（内联显示）**：当 `SelectedObject` 为 `IList` 类型时，自动切换为集合编辑模式，内联显示编辑器（无需弹出对话框）
- **集合编辑器 UserControl 化**：`CollectionEditorControl`（复杂对象列表）和 `SimpleCollectionEditorControl`（简单类型列表）可作为独立 UserControl 嵌入任意容器
- **FormulaBindingControl 独立使用**：新增 `TreeItemsSource` / `FormulaTreeProvider` 依赖属性，支持脱离 PropertyGrid 直接设置公式树数据源

#### 🔧 技术改进
- PropertyGrid 模板新增 `PART_CollectionEditorHost` / `PART_NormalHost` 双区域切换机制
- 集合元素类型自动检测：简单类型（primitives、enum、string、decimal、DateTime、TimeSpan、Guid）→ SimpleCollectionEditor，复杂类型 → CollectionEditor
- `FormulaBindingControl.LoadTreeItems()` 三级优先级：TreeItemsSource → FormulaTreeProvider → PropertyItem DataContext
- 集合编辑器控件直接操作源 `IList` 实例，每次操作后立即同步

#### 📦 新增公共类型
- `PropertyGridLib.Controls.CollectionEditorControl` — 复杂对象列表编辑器 UserControl
- `PropertyGridLib.Controls.SimpleCollectionEditorControl` — 简单类型列表编辑器 UserControl

#### 🔄 向后兼容性
- ✅ 所有现有公共 API 保持不变
- ✅ PropertyGrid 正常对象绑定行为不受影响
- ✅ FormulaBindingControl 在 PropertyGrid 内嵌用法不受影响

---

## 许可证

MIT License
