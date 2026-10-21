# 更新记录

## 1.6.5

### 新增

- **Pro 表单式排布**（`Layout = ProLayout.Section`）：分类名占左列，属性按「名称 → 说明 → 编辑器」上下排布，布尔属性显示为勾选项。
- **底部描述区**：基础网格与 Pro 均带常驻描述栏，选中属性即显示名称与说明；上方分割条可上下拖动调整高度。
- **链接自动识别**：字符串值为 `http` / `https` / `ftp` / `mailto` 时渲染为可点击文本，点击调用系统默认程序打开。
- **`[Highlight]` 关键属性标记**：基础网格显示为标题前圆点，表单式显示为左侧竖线，卡片式显示为强调色描边。
- **`[EditorChrome]` 编辑器装饰开关**：可隐藏重置按钮、修改按钮文字与提示，或单独控制类型提示。
- **图片预览编辑器**：识别 `ImageSource`、图像文件字节流，以及经 `ImagePreviewRegistry` 登记的自定义图像类型（如 `GenImage`）；缩略图点击可查看大图。
- **`PropertyGrid.Header`**：顶部标题，默认「属性」，置空则不占高度。
- 隐藏能力补齐：`ShowSearchBar`、`ShowExpandCollapse`、`ShowCategoryHeaders`。

### 改进

- 分类改为卡片式：标题透明、属性行组成带描边与圆角的卡片，基础网格、事件页与 Pro 保持同一套观感。
- 分类名、标题、关键属性标记、链接统一使用主题强调色，跟随主题与配色包。
- 同类型切换 `SelectedObject` 走原地换绑，重建耗时由 250ms 级降至个位数毫秒；跨类型仍走全量重建。
- 表单式左侧分类列宽度改为 `CategoryColumnWidth`（默认 176），窄面板下按 32% 自动收窄。
- 类型提示默认关闭（`ShowTypeHint = false`），紧凑行不再拥挤。
- 图片预览窗口改用 `HandyControl.Controls.Window`，跟随主题。

### 修复

- 公式绑定条不再叠加底色块，右侧图标区与普通编辑器一致。
- 进入公式绑定后原编辑器正确隐藏，行边缘不再露出底层控件。
- 分类列过窄导致分类名被截断、属性区被挤成竖排的问题。
- 缩略图被按钮隐式样式压扁、显示不全的问题。

## 1.6.0

- 修复连续点击「重置所有属性」崩溃（颜色编辑器改用控件独占画刷）。
- 修复可展开对象行显示完整 CLR 类型名的问题，改为友好类型名。
- 移除 `System.Windows.Forms` 依赖，对话框改用 HandyControl 原生实现。

## 1.5.0

- 修复 `FormulaSwitchControl` 在公式模式下两个输入框重叠的问题。

## 1.4.0

- 新增事件页能力（`ShowEventsPage`、`EventPicker`、`EventItem`、`EventEditorControl`）。
- 属性列表改为压平 + 虚拟化，切换选中对象的重建耗时由 206ms 降至 8ms。
- 新增 `TitlePlacement`（Small 紧凑布局）、`TitleFontWeight`、`CategoryFontWeight`。
