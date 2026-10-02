using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Dialogs
{
    /// <summary>
    /// 字典编辑器：编辑 IDictionary 的键值对。
    /// 键、值均支持两种模式——简单类型用文本框编辑并转换；复杂类型（key/value 是对象）内嵌 PropertyGrid，
    /// 因而复杂键值内部的集合/字典可继续向下嵌套编辑。
    /// 通过非泛型 IDictionary 完成增删改，键值类型从 IDictionary&lt;,&gt; 泛型参数推断。
    /// </summary>
    public partial class DictionaryEditorDialog
    {
        private readonly IDictionary _originalDictionary;
        private readonly Type _keyType;
        private readonly Type _valueType;
        // _keyIsComplex 仅指“其他复杂对象”（不含字典/列表），由内嵌 PropertyGrid 编辑
        private readonly bool _keyIsComplex;
        private readonly bool _valueIsComplex;
        // 嵌套字典/列表用“摘要 + ...”按钮弹窗编辑，避免内联导致 UI 过长
        private readonly bool _keyIsCollection;
        private readonly bool _valueIsCollection;
        private readonly ObservableCollection<DictEntry> _entries;
        private DictEntry _currentEntry;
        private bool _loading;
        private readonly bool _isReadOnly;

        /// <summary>
        /// 字典编辑器。isReadOnly=true 时进入只读查看模式：
        /// 隐藏 增/删/键值编辑输入/确定 功能，仅可浏览条目（复杂键值仍可用内嵌属性页查看）。
        /// </summary>
        public DictionaryEditorDialog(IDictionary dictionary, bool isReadOnly = false)
        {
            // 先于 InitializeComponent 赋字体：首帧渲染即为全局字体，避免先按默认值绘制再跳变
            this.FontFamily = PropertyGrid.GlobalFontFamily;
            this.FontSize = PropertyGrid.GlobalFontSize;
            _isReadOnly = isReadOnly;
            InitializeComponent();
            UiScaler.Attach(this); // 分辨率/DPI 自适应（跟随宿主全局缩放因子）

            _originalDictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            var kv = DictionaryEditHelper.GetKeyValueTypes(dictionary.GetType());
            _keyType = kv.Key;
            _valueType = kv.Value;
            // 字典/列表走弹窗（不再内联 PropertyGrid，避免嵌套导致 UI 过长），
            // 其余复杂对象仍由内嵌 PropertyGrid 就地编辑
            _keyIsCollection = DictionaryEditHelper.IsDictionaryType(_keyType) || DictionaryEditHelper.IsListType(_keyType);
            _valueIsCollection = DictionaryEditHelper.IsDictionaryType(_valueType) || DictionaryEditHelper.IsListType(_valueType);
            _keyIsComplex = !DictionaryEditHelper.IsSimpleType(_keyType) && !_keyIsCollection;
            _valueIsComplex = !DictionaryEditHelper.IsSimpleType(_valueType) && !_valueIsCollection;

            _entries = new ObservableCollection<DictEntry>();
            foreach (DictionaryEntry de in dictionary)
                _entries.Add(new DictEntry { Key = de.Key, Value = de.Value });

            ApplyEditorMode();
            ApplyLayout();
            if (_isReadOnly)
                EnterReadOnlyMode();

            EntriesListBox.ItemsSource = _entries;
            if (_entries.Count > 0)
                EntriesListBox.SelectedIndex = 0;
            else
                LoadCurrentEntry(); // 空字典时禁用键值编辑框
        }

        /// <summary>只读查看模式：隐藏增删/确定，键值输入框只读；
        /// 嵌套集合的「...」按钮保留 —— 内层弹窗同样以只读打开（OpenCollectionEditor 透传 _isReadOnly），
        /// 用户仍可逐层浏览嵌套内容（只读模式定位是「仅可浏览」）。</summary>
        private void EnterReadOnlyMode()
        {
            AddButton.Visibility = Visibility.Collapsed;
            RemoveButton.Visibility = Visibility.Collapsed;
            KeyInputBox.IsReadOnly = true;
            ValueInputBox.IsReadOnly = true;
            OkButton.Visibility = Visibility.Collapsed;
            // 仅保留"关闭"（原取消按钮），点击直接关闭不保存
            CancelButton.Content = LocalizationManager.GetString("Close");
        }

        /// <summary>按键/值是否为复杂类型，切换文本框或内嵌 PropertyGrid</summary>
        private void ApplyEditorMode()
        {
            // 键：三种模式互斥——简单(TextBox) / 嵌套集合(摘要+"..."按钮) / 其他复杂对象(PropertyGrid)
            KeyInputBox.Visibility = (!_keyIsComplex && !_keyIsCollection) ? Visibility.Visible : Visibility.Collapsed;
            KeyPropertyGridHost.Visibility = _keyIsComplex ? Visibility.Visible : Visibility.Collapsed;
            KeySummaryHost.Visibility = _keyIsCollection ? Visibility.Visible : Visibility.Collapsed;

            ValueInputBox.Visibility = (!_valueIsComplex && !_valueIsCollection) ? Visibility.Visible : Visibility.Collapsed;
            ValuePropertyGridHost.Visibility = _valueIsComplex ? Visibility.Visible : Visibility.Collapsed;
            ValueSummaryHost.Visibility = _valueIsCollection ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>键、值均为复杂对象时左右各占 50%，否则上下堆叠</summary>
        private void ApplyLayout()
        {
            EditorsHost.RowDefinitions.Clear();
            EditorsHost.ColumnDefinitions.Clear();

            if (_keyIsComplex && _valueIsComplex)
            {
                // 键、值都是对象：左右并列，各占 50%
                EditorsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                EditorsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                EditorsHost.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                Grid.SetRow(KeySection, 0); Grid.SetColumn(KeySection, 0);
                Grid.SetRow(ValueSection, 0); Grid.SetColumn(ValueSection, 1);
                KeySection.Margin = new Thickness(0, 0, 4, 0);
                ValueSection.Margin = new Thickness(4, 0, 0, 0);
            }
            else
            {
                // 上下堆叠：复杂的一侧占星高（给 PropertyGrid 足够空间），简单的一侧自适应
                EditorsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                EditorsHost.RowDefinitions.Add(new RowDefinition { Height = _keyIsComplex ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });
                EditorsHost.RowDefinitions.Add(new RowDefinition { Height = _valueIsComplex ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });

                Grid.SetRow(KeySection, 0); Grid.SetColumn(KeySection, 0);
                Grid.SetRow(ValueSection, 1); Grid.SetColumn(ValueSection, 0);
                KeySection.Margin = new Thickness(0);
                ValueSection.Margin = new Thickness(0, 8, 0, 0);
            }
        }

        #region 列表与选择

        private void EntriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentEntry = EntriesListBox.SelectedItem as DictEntry;
            LoadCurrentEntry();
        }

        private void LoadCurrentEntry()
        {
            _loading = true;
            try
            {
                if (_currentEntry == null)
                {
                    KeyInputBox.IsEnabled = false;
                    KeyInputBox.Text = string.Empty;
                    if (_keyIsComplex) KeyPropertyGrid.SelectedObject = null;
                    if (_keyIsCollection) KeySummaryText.Text = string.Empty;

                    ValueInputBox.IsEnabled = false;
                    ValueInputBox.Text = string.Empty;
                    if (_valueIsComplex) ValuePropertyGrid.SelectedObject = null;
                    if (_valueIsCollection) ValueSummaryText.Text = string.Empty;
                    return;
                }

                // 键
                if (_keyIsComplex)
                {
                    if (_currentEntry.Key == null)
                        _currentEntry.Key = DictionaryEditHelper.CreateDefaultValue(_keyType);
                    KeyPropertyGrid.SelectedObject = _currentEntry.Key;
                }
                else if (_keyIsCollection)
                {
                    // 嵌套字典/列表：显示当前条目数；确保非空实例，便于弹窗就地编辑
                    if (_currentEntry.Key == null)
                        _currentEntry.Key = DictionaryEditHelper.CreateDefaultValue(_keyType);
                    KeySummaryText.Text = DictionaryEditHelper.GetCollectionSummary(_currentEntry.Key);
                }
                else
                {
                    KeyInputBox.IsEnabled = true;
                    KeyInputBox.Text = _currentEntry.Key?.ToString() ?? string.Empty;
                }

                // 值
                if (_valueIsComplex)
                {
                    if (_currentEntry.Value == null)
                        _currentEntry.Value = DictionaryEditHelper.CreateDefaultValue(_valueType);
                    ValuePropertyGrid.SelectedObject = _currentEntry.Value;
                }
                else if (_valueIsCollection)
                {
                    if (_currentEntry.Value == null)
                        _currentEntry.Value = DictionaryEditHelper.CreateDefaultValue(_valueType);
                    ValueSummaryText.Text = DictionaryEditHelper.GetCollectionSummary(_currentEntry.Value);
                }
                else
                {
                    ValueInputBox.IsEnabled = true;
                    ValueInputBox.Text = _currentEntry.Value?.ToString() ?? string.Empty;
                }
            }
            finally { _loading = false; }
        }

        #endregion

        #region 键/值提交

        private void KeyInputBox_LostFocus(object sender, RoutedEventArgs e) => CommitKey();

        private void ValueInputBox_LostFocus(object sender, RoutedEventArgs e) => CommitValue();

        private void CommitKey()
        {
            // 复杂键/嵌套集合键：由内嵌 PropertyGrid 或弹窗就地编辑，无需文本提交
            if (_loading || _currentEntry == null || _keyIsComplex || _keyIsCollection) return;
            try
            {
                var newKey = DictionaryEditHelper.ConvertFromString(KeyInputBox.Text, _keyType);
                if (Equals(_currentEntry.Key, newKey)) return;
                if (_entries.Any(x => !ReferenceEquals(x, _currentEntry) && Equals(x.Key, newKey)))
                {
                    Warn(LocalizationManager.GetString("DuplicateKey", newKey?.ToString() ?? "(null)"));
                    KeyInputBox.Text = _currentEntry.Key?.ToString() ?? string.Empty;
                    return;
                }
                _currentEntry.Key = newKey;
            }
            catch (Exception ex)
            {
                Error(LocalizationManager.GetString("ConvertFailed", ex.Message));
                KeyInputBox.Text = _currentEntry.Key?.ToString() ?? string.Empty;
            }
        }

        private void CommitValue()
        {
            if (_loading || _currentEntry == null || _valueIsComplex || _valueIsCollection) return;
            try
            {
                _currentEntry.Value = DictionaryEditHelper.ConvertFromString(ValueInputBox.Text, _valueType);
            }
            catch (Exception ex)
            {
                Error(LocalizationManager.GetString("ConvertFailed", ex.Message));
                ValueInputBox.Text = _currentEntry.Value?.ToString() ?? string.Empty;
            }
        }

        #endregion

        #region 嵌套集合弹窗编辑

        /// <summary>嵌套字典/列表的键：弹窗编辑，确认后刷新摘要</summary>
        private void KeyEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntry == null || !_keyIsCollection) return;
            if (_currentEntry.Key == null)
                _currentEntry.Key = DictionaryEditHelper.CreateDefaultValue(_keyType);

            if (DictionaryEditHelper.OpenCollectionEditor(this, _currentEntry.Key, _isReadOnly))
            {
                KeySummaryText.Text = DictionaryEditHelper.GetCollectionSummary(_currentEntry.Key);
            }
        }

        /// <summary>嵌套字典/列表的值：弹窗编辑，确认后刷新摘要</summary>
        private void ValueEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntry == null || !_valueIsCollection) return;
            if (_currentEntry.Value == null)
                _currentEntry.Value = DictionaryEditHelper.CreateDefaultValue(_valueType);

            if (DictionaryEditHelper.OpenCollectionEditor(this, _currentEntry.Value, _isReadOnly))
            {
                ValueSummaryText.Text = DictionaryEditHelper.GetCollectionSummary(_currentEntry.Value);
            }
        }

        #endregion

        #region 增删

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entry = new DictEntry
                {
                    Key = DictionaryEditHelper.MakeUniqueKey(_entries, _keyType),
                    Value = DictionaryEditHelper.CreateDefaultValue(_valueType)
                };
                _entries.Add(entry);
                EntriesListBox.SelectedItem = entry;
                // 简单键时聚焦文本框便于立即输入
                if (!_keyIsComplex && !_keyIsCollection)
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        KeyInputBox.Focus();
                        KeyInputBox.SelectAll();
                    }), System.Windows.Threading.DispatcherPriority.Input);
            }
            catch (Exception ex)
            {
                Error(LocalizationManager.GetString("AddFailed", ex.Message));
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntry == null)
            {
                HandyControl.Controls.MessageBox.Show(LocalizationManager.GetString("SelectItemToDelete"),
                    LocalizationManager.GetString("Tip"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var idx = EntriesListBox.SelectedIndex;
            _entries.Remove(_currentEntry);
            _currentEntry = null;
            if (_entries.Count > 0)
                EntriesListBox.SelectedIndex = Math.Min(idx, _entries.Count - 1);
        }

        #endregion

        #region 确定/取消

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CommitKey();
            CommitValue();

            var dup = DictionaryEditHelper.FindDuplicateKey(_entries);
            if (dup != null)
            {
                Warn(LocalizationManager.GetString("DuplicateKey", dup.Key?.ToString() ?? "(null)"));
                return;
            }

            try
            {
                _originalDictionary.Clear();
                foreach (var entry in _entries)
                    _originalDictionary.Add(entry.Key, entry.Value);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        private static void Warn(string msg) =>
            HandyControl.Controls.MessageBox.Show(msg, LocalizationManager.GetString("Tip"),
                MessageBoxButton.OK, MessageBoxImage.Warning);

        private static void Error(string msg) =>
            HandyControl.Controls.MessageBox.Show(msg, LocalizationManager.GetString("Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
