using System.ComponentModel;

namespace PropertyGridLib.Localization
{
    /// <summary>
    /// 本地化代理：供 XAML 绑定使用，语言切换时自动刷新所有绑定。
    /// </summary>
    public class LocalizationProxy : INotifyPropertyChanged
    {
        private static LocalizationProxy _instance;
        public static LocalizationProxy Instance => _instance ?? (_instance = new LocalizationProxy());

        /// <summary>索引器：通过 key 获取本地化字符串</summary>
        public string this[string key] => LocalizationManager.GetString(key);

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>语言切换时调用，刷新所有索引器绑定</summary>
        public void RaiseLanguageChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }
}
