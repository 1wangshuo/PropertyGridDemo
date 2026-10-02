# Tools

TeamPLT 通用工具集。仓库约定：**一个工具一个分支**，各分支提交历史互相独立。

| 分支 | 内容 |
| --- | --- |
| `PropertyGrid` | WPF 属性网格控件库 PropertyGridLib + Demo 工程 |

## 拉取某个工具

```bash
git clone -b PropertyGrid --single-branch http://49.235.130.76/TeamPLT/Tools.git PropertyGrid
```

## PropertyGrid 文档入口

- [`PropertyGridLib/README.md`](PropertyGridLib/README.md) — 控件库完整文档（属性定义、编辑器类型、主题、多语言、公式绑定、PropertyGridPro）
- [`PropertyGridDemo/README.md`](PropertyGridDemo/README.md) — Demo 工程与演示场景说明
- [`PropertyGridDemo/skill/SKILL.md`](PropertyGridDemo/skill/SKILL.md) — skill 文档总入口
  - 快速开始与基础用法 / 特性清单 / 编辑器类型总览 / 主题与多语言
  - 公式绑定 / PropertyGridPro 平铺参数面板 / 示例模型与演示场景 / 常见问题 FAQ

## 目录结构

```
PropertyGrid.sln (PropertyGridDemo.sln)
├── PropertyGridLib/   控件库源码
├── PropertyGridDemo/  Demo 工程 + skill 文档
└── nuget/             历史发布的 nupkg 包
```
