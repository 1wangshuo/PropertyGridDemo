using PropertyGridLib.Localization;

namespace PropertyGridLib
{
    /// <summary>
    /// 属性本地化接口（v2，Pull 模式）：允许数据对象动态提供属性名、描述和分类的显示文本。
    ///
    /// 设计约定：
    /// - 不再传入语言参数——语言由 <see cref="LocalizationManager.CurrentCulture"/> 全局决定，
    ///   实现方需要多语言时自行按 CurrentCulture 返回对应文本；
    /// - 所有方法返回 null 时回退到特性值（[DisplayName]/[Description]/[Category]），
    ///   回退文本会再经过全局翻译钩子 <see cref="LocalizationManager.TextResolver"/> 取词；
    /// - 定位收窄：仅当属性文本来源于运行时动态数据（如数据库字段名、设备上报通道名）时才实现本接口，
    ///   静态文案一律写在特性里，交给"中文原文作 key"的全局语言包体系，不要在代码里 hardcode 译文字典。
    /// </summary>
    public interface IPropertyLocalization
    {
        /// <summary>获取属性的显示名称（当前语言下），返回 null 则回退到特性值</summary>
        string GetDisplayName(string propertyName);

        /// <summary>获取属性的描述文本（当前语言下），返回 null 则回退到特性值</summary>
        string GetDescription(string propertyName);

        /// <summary>获取属性的分类名（当前语言下），返回 null 则回退到特性值</summary>
        string GetCategory(string propertyName);
    }
}
