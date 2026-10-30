using System;
using System.Collections.Generic;

namespace PropertyGridLib.Localization
{
    /// <summary>
    /// 支持的语言
    /// </summary>
    public enum Language
    {
        ZhCN,
        EnUS
    }

    /// <summary>
    /// 本地化管理器：管理类库内部所有用户可见字符串的多语言切换。
    /// 默认中文，除中文外均显示英文。
    /// </summary>
    public static class LocalizationManager
    {
        /// <summary>当前区域 culture（默认 "zh-CN"。zh* 前缀 → 中文词典，其余 → 英文词典）</summary>
        private static string _currentCulture = "zh-CN";

        private static readonly Dictionary<string, string> ZhCNStrings = new Dictionary<string, string>
        {
            // PropertyGrid 主界面
            { "SearchPlaceholder", "搜索属性..." },
            { "SelectBindingSource", "选择绑定源" },
            { "SelectItems", "选择项" },
            { "SelectMultiple", "选择多项" },
            { "ResetToDefault", "重置为初始值" },
            { "ExpandAll", "展开" },
            { "CollapseAll", "折叠" },
            { "FormulaPlaceholder", "公式绑定..." },
            { "FormulaPlaceholderCompact", "公式..." },
            { "CollapseBindingEditor", "收起绑定" },
            // 文件/目录对话框
            { "SelectFile", "选择文件" },
            { "SelectFolder", "选择文件夹" },
            { "AllFiles", "所有文件" },
            { "Files", "文件" },
            // 计数与分类
            { "ItemsCount", "[{0} 项]" },
            { "Misc", "杂项" },
            // MultiSelect 摘要
            { "NotSelected", "（未选择）" },
            { "SelectedCount", "已选 {0} 项" },
            // CollectionEditorDialog
            { "CollectionEditorTitle", "集合编辑器" },
            { "CollectionItems", "集合项" },
            { "OpenCollectionEditor", "打开集合编辑器" },
            { "EmptyCollection", "（空集合）" },
            { "Properties", "属性" },
            { "Add", "添加" },
            { "Copy", "复制" },
            { "Remove", "删除" },
            { "OK", "确定" },
            { "Cancel", "取消" },
            { "Close", "关闭" },
            // SimpleCollectionEditorDialog
            { "EditCollectionTitle", "编辑集合" },
            { "InputPlaceholder", "输入新值..." },
            { "MoveUp", "上移" },
            { "MoveDown", "下移" },
            // DictionaryEditorDialog
            { "DictionaryEditorTitle", "字典编辑器" },
            { "DictEntries", "字典条目" },
            { "DictKey", "键 (Key)" },
            { "DictValue", "值 (Value)" },
            { "OpenDictionaryEditor", "打开字典编辑器" },
            { "EmptyDictionary", "（空字典）" },
            { "DuplicateKey", "键 “{0}” 已存在，请使用唯一的键" },
            // 消息提示
            { "Tip", "提示" },
            { "Error", "错误" },
            { "AddFailed", "添加失败：{0}" },
            { "CopyFailed", "复制失败：{0}" },
            { "ConvertFailed", "转换失败：{0}" },
            { "SelectItemToDelete", "请先选择要删除的项" },
            { "SelectItemToCopy", "请先选择要复制的项" },
            { "NoDefaultConstructor", "类型 {0} 没有无参构造函数" },
            { "PleaseSelectFolder", "请选择一个文件夹" },
            { "FolderNotExistCreateIt", "文件夹不存在，是否创建？" },
            // FolderBrowserDialog
            { "FolderBrowserTitle", "选择文件夹" },
            { "SelectedPath", "选中路径：" },
            { "CreateFolderFailed", "创建文件夹失败：{0}" },
            { "LoadDrivesFailed", "加载驱动器失败：{0}" },
            // 文件夹右键菜单（新建 / 重命名 / 删除）
            { "NewFolder", "新建文件夹" },
            { "RenameFolder", "重命名" },
            { "DeleteFolder", "删除" },
            { "FolderName", "文件夹名称" },
            { "FolderNameEmpty", "文件夹名称不能为空" },
            { "InvalidFolderName", "名称包含非法字符（\\ / : * ? \" < > |）" },
            { "FolderAlreadyExists", "文件夹 “{0}” 已存在" },
            { "ConfirmDeleteFolder", "确定删除文件夹 “{0}” 及其全部内容吗？" },
            { "RenameFolderFailed", "重命名失败：{0}" },
            { "DeleteFolderFailed", "删除失败：{0}" },
            // PropertyGridPro（平铺参数配置面板）
            { "ProNoResult", "没有匹配的属性" },
            { "ProMatchedCount", "共 {0} 项" },
            // ToggleSwitch 状态文字（[ToggleSwitch] 未指定 OnText/OffText 时的库默认值）
            { "ToggleOn", "开" },
            { "ToggleOff", "关" },
            // 1.4.0 事件页 / EventPicker
            { "EventsCategory", "事件" },
            { "PropertiesTab", "属性" },
            { "EventsTab", "事件" },
            { "EventSearchPlaceholder", "搜索事件..." },
            { "EditHandler", "编辑事件代码" },
            { "NoHandler", "<未绑定>" },
            // 图片预览
            { "ImagePreviewTitle", "图像预览" },
            { "NoPreview", "无预览" },
        };

        private static readonly Dictionary<string, string> EnUSStrings = new Dictionary<string, string>
        {
            // PropertyGrid main UI
            { "SearchPlaceholder", "Search properties..." },
            { "SelectBindingSource", "Select Binding Source" },
            { "SelectItems", "Select Items" },
            { "SelectMultiple", "Select Multiple" },
            { "ResetToDefault", "Reset to default" },
            { "ExpandAll", "Expand" },
            { "CollapseAll", "Collapse" },
            { "FormulaPlaceholder", "Formula binding..." },
            { "FormulaPlaceholderCompact", "Formula..." },
            { "CollapseBindingEditor", "Collapse binding" },
            // File/Directory dialogs
            { "SelectFile", "Select File" },
            { "SelectFolder", "Select Folder" },
            { "AllFiles", "All Files" },
            { "Files", "Files" },
            // Count & Category
            { "ItemsCount", "[{0} items]" },
            { "Misc", "Miscellaneous" },
            // MultiSelect summary
            { "NotSelected", "(None)" },
            { "SelectedCount", "{0} items selected" },
            // CollectionEditorDialog
            { "CollectionEditorTitle", "Collection Editor" },
            { "CollectionItems", "Collection Items" },
            { "OpenCollectionEditor", "Open collection editor" },
            { "EmptyCollection", "(empty)" },
            { "Properties", "Properties" },
            { "Add", "Add" },
            { "Copy", "Copy" },
            { "Remove", "Remove" },
            { "OK", "OK" },
            { "Cancel", "Cancel" },
            { "Close", "Close" },
            // SimpleCollectionEditorDialog
            { "EditCollectionTitle", "Edit Collection" },
            { "InputPlaceholder", "Enter new value..." },
            { "MoveUp", "Move Up" },
            { "MoveDown", "Move Down" },
            // DictionaryEditorDialog
            { "DictionaryEditorTitle", "Dictionary Editor" },
            { "DictEntries", "Entries" },
            { "DictKey", "Key" },
            { "DictValue", "Value" },
            { "OpenDictionaryEditor", "Open dictionary editor" },
            { "EmptyDictionary", "(empty)" },
            { "DuplicateKey", "Key \"{0}\" already exists. Keys must be unique." },
            // Messages
            { "Tip", "Tip" },
            { "Error", "Error" },
            { "AddFailed", "Add failed: {0}" },
            { "CopyFailed", "Copy failed: {0}" },
            { "ConvertFailed", "Convert failed: {0}" },
            { "SelectItemToDelete", "Please select an item to delete first" },
            { "SelectItemToCopy", "Please select an item to copy first" },
            { "NoDefaultConstructor", "Type {0} has no parameterless constructor" },
            { "PleaseSelectFolder", "Please select a folder" },
            { "FolderNotExistCreateIt", "Folder does not exist. Create it?" },
            // FolderBrowserDialog
            { "FolderBrowserTitle", "Select Folder" },
            { "SelectedPath", "Selected Path:" },
            { "CreateFolderFailed", "Create folder failed: {0}" },
            { "LoadDrivesFailed", "Load drives failed: {0}" },
            // Folder context menu (new / rename / delete)
            { "NewFolder", "New Folder" },
            { "RenameFolder", "Rename" },
            { "DeleteFolder", "Delete" },
            { "FolderName", "Folder name" },
            { "FolderNameEmpty", "Folder name cannot be empty" },
            { "InvalidFolderName", "Name contains invalid characters (\\ / : * ? \" < > |)" },
            { "FolderAlreadyExists", "Folder \"{0}\" already exists" },
            { "ConfirmDeleteFolder", "Delete folder \"{0}\" and all its contents?" },
            { "RenameFolderFailed", "Rename failed: {0}" },
            { "DeleteFolderFailed", "Delete failed: {0}" },
            // PropertyGridPro (flat parameter configuration panel)
            { "ProNoResult", "No matching properties" },
            { "ProMatchedCount", "{0} item(s)" },
            // ToggleSwitch state text (library defaults when [ToggleSwitch] has no OnText/OffText)
            { "ToggleOn", "ON" },
            { "ToggleOff", "OFF" },
            // 1.4.0 Events page / EventPicker
            { "EventsCategory", "Events" },
            { "PropertiesTab", "Properties" },
            { "EventsTab", "Events" },
            { "EventSearchPlaceholder", "Search events..." },
            { "EditHandler", "Edit event code" },
            { "NoHandler", "<not bound>" },
            // Image preview
            { "ImagePreviewTitle", "Image Preview" },
            { "NoPreview", "No preview" },
        };

        /// <summary>
        /// 当前区域 culture（如 "zh-CN"、"en-US"，开集——支持任意语言标识，与宿主语言包体系对齐）。
        /// 切换时触发 LanguageChanged 并通知 LocalizationProxy，
        /// 打开中的 PropertyGrid 会通过 UpdateLocalization() 原地刷新文本。
        /// </summary>
        public static string CurrentCulture
        {
            get => _currentCulture;
            set
            {
                var v = string.IsNullOrWhiteSpace(value) ? "zh-CN" : value.Trim().Replace('_', '-');
                if (_currentCulture == v) return;
                _currentCulture = v;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
                LocalizationProxy.Instance.RaiseLanguageChanged();
            }
        }

        /// <summary>当前语言枚举（兼容旧调用方式）。新代码请使用 <see cref="CurrentCulture"/>。</summary>
        [Obsolete("请改用 CurrentCulture（culture 字符串开集，如 \"en-US\"），枚举仅保留兼容")]
        public static Language CurrentLanguage
        {
            get => _currentCulture.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? Language.ZhCN : Language.EnUS;
            set => CurrentCulture = value == Language.ZhCN ? "zh-CN" : "en-US";
        }

        /// <summary>
        /// 全局文本翻译钩子：宿主程序启动时注入（例如平台侧 LanguageManager 的 L.T 取词函数）。
        /// 特性回退路径读取的 [DisplayName]/[Description]/[Category] 文本会经过它翻译；
        /// 未设置或返回 null 时使用原文。语言切换后由 UpdateLocalization() 自动重新取词。
        /// </summary>
        public static Func<string, string> TextResolver;

        // ===== 属性/事件显示名翻译表（宿主注册，1.4.1 新增）=====
        // 反射场景（如 WPF 设计器把控件实例交给 SelectedObject）没有 [DisplayName] 可用，
        // 属性名只能取 PropertyInfo.Name 原文。宿主把「CLR 名 → 显示名」词条注册进来，
        // PropertyItem / EventItem 在无 [DisplayName] 时查此表（仅中文环境命中，其余语言用原名）。
        private static readonly Dictionary<string, string> PropertyDisplayNames
            = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>注册一条属性/事件显示名翻译（同名覆盖）。name=CLR 名（如 "ToolTip"），displayName=显示名（如 "工具提示"）。</summary>
        public static void RegisterPropertyName(string name, string displayName)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(displayName)) return;
            PropertyDisplayNames[name] = displayName;
        }

        /// <summary>批量注册属性/事件显示名翻译（同名覆盖）。键为 CLR 名，值为显示名。</summary>
        public static void RegisterPropertyNames(IEnumerable<KeyValuePair<string, string>> map)
        {
            if (map == null) return;
            foreach (var kv in map)
                RegisterPropertyName(kv.Key, kv.Value);
        }

        /// <summary>清空属性/事件显示名翻译表（宿主想回到英文原名时调用）。注册的词条会随语言切换自动生效/失效。</summary>
        public static void ClearPropertyNames() => PropertyDisplayNames.Clear();

        /// <summary>
        /// 查属性/事件显示名：中文环境查翻译表，命中返回显示名；
        /// 非中文环境或未命中返回 null（由调用方回退到 [DisplayName] 或原始名称）。
        /// </summary>
        public static string TryGetPropertyName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (!_currentCulture.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) return null;
            return PropertyDisplayNames.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>语言切换事件</summary>
        public static event EventHandler LanguageChanged;

        /// <summary>获取本地化字符串（库自身界面文本；zh* → 中文词典，其余 → 英文词典）</summary>
        public static string GetString(string key)
        {
            var dict = _currentCulture.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? ZhCNStrings : EnUSStrings;
            return dict.TryGetValue(key, out var value) ? value : key;
        }

        /// <summary>获取本地化字符串（带格式化参数）</summary>
        public static string GetString(string key, params object[] args)
        {
            var template = GetString(key);
            return args.Length > 0 ? string.Format(template, args) : template;
        }
    }
}
