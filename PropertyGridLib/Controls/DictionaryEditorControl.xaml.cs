using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PropertyGridLib.Dialogs;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 内联字典编辑器：当 PropertyGrid.SelectedObject 直接是 IDictionary 时，
    /// 用本控件就地编辑键值对（对标 <see cref="CollectionEditorControl"/> 之于列表）。
    /// 键、值均支持简单类型（文本框）或复杂类型（内嵌 PropertyGrid，可继续嵌套集合/字典）。
    /// 结构性变更（增/删/改键值）会实时写回原字典。
    /// </summary>
    public partial class DictionaryEditorControl : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IDictionary), typeof(DictionaryEditorControl),
                new PropertyMetadata(null, OnItemsChanged));

        /// <summary>要编辑的字典（就地写回）</summary>
        public IDictionary Items
        {
            get => (IDictionary)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        /// <summary>
        /// 紧凑形态：true = 只显示一条「值框 + ...」（点 ... 打开字典编辑器弹窗）；
        /// false（默认）= 当前完整内联编辑面板。
        /// </summary>
        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register(nameof(Compact), typeof(bool), typeof(DictionaryEditorControl),
                new PropertyMetadata(false, (d, e) => ((DictionaryEditorControl)d).ApplyCompact()));

        /// <summary>紧凑形态</summary>
        public bool Compact
        {
            get => (bool)GetValue(CompactProperty);
            set => SetValue(CompactProperty, value);
        }

        private void ApplyCompact()
        {
            if (PanelHost == null || CompactBar == null) return;
            PanelHost.Visibility = Compact ? Visibility.Collapsed : Visibility.Visible;
            CompactBar.Visibility = Compact ? Visibility.Visible : Visibility.Collapsed;
            UpdateCompactSummary();
        }

        private void UpdateCompactSummary()
        {
            if (CompactSummary == null) return;
            var n = Items?.Count ?? 0;
            CompactSummary.Text = n == 0 ? "（空字典）" : $"[{n} 项]";
        }

        /// <summary>紧凑形态的「...」：打开字典编辑器弹窗（与网格行同款对话框），关闭后刷新摘要</summary>
        private void CompactEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Items == null) return;
            var dlg = new DictionaryEditorDialog(Items);
            var owner = Window.GetWindow(this);
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
            LoadDictionary();
            UpdateCompactSummary();
        }

        private Type _keyType;
        private Type _valueType;
        // _keyIsComplex 仅指“其他复杂对象”（不含字典/列表），由内嵌 PropertyGrid 编辑
        private bool _keyIsComplex;
        private bool _valueIsComplex;
        // 嵌套字典/列表用“摘要 + ...”按钮弹窗编辑，避免内联导致 UI 过长
        private bool _keyIsCollection;
        private bool _valueIsCollection;
        private ObservableCollection<DictEntry> _entries;
        private DictEntry _currentEntry;
        private bool _loading;

        public DictionaryEditorControl()
        {
            InitializeComponent();
            // 复杂键/值由内嵌属性页就地修改（对象引用不变）时不走 DictEntry 的 setter，
            // 列表行会停在旧文本 —— 订阅两张属性页的值变更，改完即重算行显示（只刷显示层）。
            KeyPropertyGrid.PropertyValueChanged += (s, e) => DictEntry.RefreshDisplayTexts(_entries);
            ValuePropertyGrid.PropertyValueChanged += (s, e) => DictEntry.RefreshDisplayTexts(_entries);
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((DictionaryEditorControl)d).LoadDictionary();

        private void LoadDictionary()
        {
            ApplyCompact();
            var dict = Items;
            if (dict == null)
            {
                _entries = null;
                _currentEntry = null;
                EntriesListBox.ItemsSource = null;
                KeyPropertyGrid.SelectedObject = null;
                ValuePropertyGrid.SelectedObject = null;
                return;
            }

            var kv = DictionaryEditHelper.GetKeyValueTypes(dict.GetType());
            _keyType = kv.Key;
            _valueType = kv.Value;
            // 字典/列表走弹窗（不再内联 PropertyGrid，避免嵌套导致 UI 过长），
            // 其余复杂对象仍由内嵌 PropertyGrid 就地编辑
            _keyIsCollection = DictionaryEditHelper.IsDictionaryType(_keyType) || DictionaryEditHelper.IsListType(_keyType);
            _valueIsCollection = DictionaryEditHelper.IsDictionaryType(_valueType) || DictionaryEditHelper.IsListType(_valueType);
            _keyIsComplex = !DictionaryEditHelper.IsSimpleType(_keyType) && !_keyIsCollection;
            _valueIsComplex = !DictionaryEditHelper.IsSimpleType(_valueType) && !_valueIsCollection;

            _entries = new ObservableCollection<DictEntry>();
            foreach (DictionaryEntry de in dict)
                _entries.Add(new DictEntry { Key = de.Key, Value = de.Value });

            ApplyEditorMode();
            ApplyLayout();

            EntriesListBox.ItemsSource = _entries;
            if (_entries.Count > 0)
                EntriesListBox.SelectedIndex = 0;
            else
                LoadCurrentEntry();
        }

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

        /// <summary>把当前条目集合写回原字典（存在重复键时跳过，避免 Add 抛异常）</summary>
        private void SyncBack()
        {
            var dict = Items;
            if (dict == null || _entries == null) return;
            if (DictionaryEditHelper.FindDuplicateKey(_entries) != null) return;
            try
            {
                dict.Clear();
                foreach (var entry in _entries)
                    dict.Add(entry.Key, entry.Value);
            }
            catch
            {
                // 写回失败时保持 UI 不崩溃（例如只读字典）
            }
        }

        #region 列表与选择

        private void EntriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 切换条目前先写回：复杂键/值经内嵌 PropertyGrid 就地编辑后，需重新哈希写回原字典
            SyncBack();
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
                SyncBack();
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
                SyncBack();
            }
            catch (Exception ex)
            {
                Error(LocalizationManager.GetString("ConvertFailed", ex.Message));
                ValueInputBox.Text = _currentEntry.Value?.ToString() ?? string.Empty;
            }
        }

        #endregion

        #region 嵌套集合弹窗编辑

        /// <summary>嵌套字典/列表的键：弹窗编辑，确认后刷新摘要并写回原字典</summary>
        private void KeyEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntry == null || !_keyIsCollection) return;
            if (_currentEntry.Key == null)
                _currentEntry.Key = DictionaryEditHelper.CreateDefaultValue(_keyType);

            if (DictionaryEditHelper.OpenCollectionEditor(Window.GetWindow(this), _currentEntry.Key))
            {
                KeySummaryText.Text = DictionaryEditHelper.GetCollectionSummary(_currentEntry.Key);
                SyncBack();
            }
        }

        /// <summary>嵌套字典/列表的值：弹窗编辑，确认后刷新摘要并写回原字典</summary>
        private void ValueEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEntry == null || !_valueIsCollection) return;
            if (_currentEntry.Value == null)
                _currentEntry.Value = DictionaryEditHelper.CreateDefaultValue(_valueType);

            if (DictionaryEditHelper.OpenCollectionEditor(Window.GetWindow(this), _currentEntry.Value))
            {
                ValueSummaryText.Text = DictionaryEditHelper.GetCollectionSummary(_currentEntry.Value);
                SyncBack();
            }
        }

        #endregion

        #region 增删

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (_entries == null) return;
            try
            {
                var entry = new DictEntry
                {
                    Key = DictionaryEditHelper.MakeUniqueKey(_entries, _keyType),
                    Value = DictionaryEditHelper.CreateDefaultValue(_valueType)
                };
                _entries.Add(entry);
                SyncBack();
                EntriesListBox.SelectedItem = entry;
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
            SyncBack();
            if (_entries.Count > 0)
                EntriesListBox.SelectedIndex = Math.Min(idx, _entries.Count - 1);
            else
                LoadCurrentEntry();
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
