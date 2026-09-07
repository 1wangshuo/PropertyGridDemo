# PropertyGridDemo

[PropertyGridLib](https://www.nuget.org/packages/PropertyGridLib) 的官方演示项目 — WPF PropertyGrid 控件库，仿 WinForms PropertyGrid，基于 HandyControl UI 库。

> **作者：WangShuo** | 目标框架：.NET Framework 4.8

## 项目介绍

本 Demo 以「智能监控摄像头配置」为场景，完整演示 PropertyGridLib 的全部编辑器类型与扩展机制：

| 演示分类 | 内容 |
|---------|------|
| I. 基本信息 | string / bool / enum / DateTime / 可空类型 |
| II. 外观与显示 | 颜色编辑器（`Color` / `Color?` / `SolidColorBrush`）、数值滑块 `[NumberSlider]` |
| III. 网络配置 | 常规类型编辑 |
| IV. 存储路径 | 文件选择 `[FilePath]`、文件夹选择 `[DirectoryPath]` |
| V. 高级配置 | TypeConverter 下拉列表、多选列表 `[MultiSelect]` |
| VI. 检测区域 | 复杂对象展开编辑（内嵌 PropertyGrid） |
| VII. 集合编辑器 | 简单类型集合、复杂对象集合 `[CollectionEditor]` |
| VIII. 字典编辑器 | 简单键值、复杂对象值、嵌套字典自动弹窗 |
| IX. 公式绑定 | `FormulaBound<T>` + `[FormulaEditor]`，树形绑定源选择 |
| X. 操作按钮 | `[Button]` 命令按钮 + 反射调用 + 全局点击事件 |
| XI. 宿主扩展编辑器 | `RegisterEditor<TAttr>` 自定义「数值 + 单位下拉」编辑器 |
| XII. 只读与隐藏 | `[ReadOnly]` / `[Browsable(false)]` |
| XIII. 自然排序 | 属性排序演示 |

此外还演示了：

- **深色 / 浅色主题切换**（HandyControl `Theme.Skin`）
- **中英文双语切换**（`IPropertyLocalization` + `LocalizationManager`）
- **全局字体 / 字号设置**（PropertyGrid 内所有弹窗编辑器同步跟随）
- **控件独立使用**：集合编辑器、字典编辑器、公式绑定控件脱离 PropertyGrid 单独嵌入窗口

## 运行

```bash
git clone https://github.com/1wangshuo/PropertyGridDemo.git
cd PropertyGridDemo
dotnet build PropertyGridDemo.csproj
```

用 Visual Studio 2022 打开 `PropertyGridDemo.csproj` 直接 F5 运行即可。

## 相关链接

| 资源 | 地址 |
|------|------|
| **控件库源码文档** | https://github.com/1wangshuo/PropertyGridLib |
| **NuGet 包** | `Install-Package PropertyGridLib -Version 1.2.0` |

```xml
<PackageReference Include="PropertyGridLib" Version="1.2.0" />
```

## 许可证

MIT License
