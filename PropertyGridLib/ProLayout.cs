namespace PropertyGridLib
{
    /// <summary>
    /// PropertyGridPro 的排布形态。
    /// </summary>
    public enum ProLayout
    {
        /// <summary>
        /// 卡片式（默认）：每个属性一张独立圆角卡片，属性名 + 说明贴左、值编辑器右对齐固定宽度。
        /// 适合窄面板、参数一屏扫读。
        /// </summary>
        Card,

        /// <summary>
        /// 表单式（对齐 VS 2026 选项页）：分类名在左做大标题、区块之间用分隔线断开，
        /// 每个属性为「名称 → 说明 → 编辑器」上下排布、编辑器左对齐；
        /// 布尔属性渲染为「复选框 + 名称 + 说明」的勾选项。
        /// 适合宽面板里的设置页（一列宽表单，逐项往下读）。
        /// </summary>
        Section
    }
}
