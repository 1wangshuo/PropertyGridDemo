using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PropertyGridLib.Dialogs;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    public partial class CollectionEditorControl : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IList), typeof(CollectionEditorControl),
                new PropertyMetadata(null, OnItemsChanged));

        private List<object> _items;
        private Type _itemType;

        public IList Items
        {
            get => (IList)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        /// <summary>
        /// 紧凑形态：true = 只显示一条「值框 + ...」（点 ... 打开集合编辑器弹窗）；
        /// false（默认）= 当前完整内联编辑面板。独立使用时占一行高度就设 Compact="True"。
        /// </summary>
        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register(nameof(Compact), typeof(bool), typeof(CollectionEditorControl),
                new PropertyMetadata(false, (d, e) => ((CollectionEditorControl)d).ApplyCompact()));

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
            CompactSummary.Text = n == 0 ? "（空集合）" : $"[{n} 项]";
        }

        /// <summary>紧凑形态的「...」：打开集合编辑器弹窗（与网格行同款对话框），关闭后刷新摘要</summary>
        private void CompactEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Items == null) return;
            var dlg = new Dialogs.CollectionEditorDialog(Items);
            var owner = Window.GetWindow(this);
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
            LoadItems();
            UpdateCompactSummary();
        }

        public CollectionEditorControl()
        {
            InitializeComponent();
            // 内嵌属性页改的是条目对象本身（引用不变），列表行显示文本是加载时的一次性快照，
            // 不订阅值变更就会一直停在旧名称；每次值变更按当前对象重算显示文本（只刷显示层）。
            ItemPropertyGrid.PropertyValueChanged += (s, e) =>
                ListBoxItemData.RefreshDisplayTexts(ItemsListBox.ItemsSource as IEnumerable);
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CollectionEditorControl)d).LoadItems();
        }

        private void LoadItems()
        {
            ApplyCompact();
            var collection = Items;
            if (collection == null)
            {
                _items = null;
                _itemType = null;
                ItemsListBox.ItemsSource = null;
                ItemPropertyGrid.SelectedObject = null;
                UpdateCompactSummary();
                return;
            }

            _itemType = GetItemType(collection);
            _items = new List<object>();
            for (int i = 0; i < collection.Count; i++)
                _items.Add(collection[i]);

            RefreshListBox();
        }

        private Type GetItemType(IList collection)
        {
            if (collection == null) return typeof(object);
            var type = collection.GetType();
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                if (args.Length > 0) return args[0];
            }
            if (collection.Count > 0)
                return collection[0]?.GetType() ?? typeof(object);
            return typeof(object);
        }

        private void RefreshListBox()
        {
            var items = new List<ListBoxItemData>();
            for (int i = 0; i < _items.Count; i++)
            {
                items.Add(new ListBoxItemData
                {
                    Index = i + 1,
                    DisplayText = _items[i]?.ToString() ?? "(null)",
                    Value = _items[i]
                });
            }
            ItemsListBox.ItemsSource = items;

            if (items.Count > 0)
                ItemsListBox.SelectedIndex = items.Count - 1;
        }

        private void SyncBack()
        {
            var collection = Items;
            if (collection == null) return;
            collection.Clear();
            foreach (var item in _items)
                collection.Add(item);
        }

        private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsListBox.SelectedItem is ListBoxItemData data)
                ItemPropertyGrid.SelectedObject = data.Value;
            else
                ItemPropertyGrid.SelectedObject = null;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newItem = CreateDefaultInstance(_itemType);
                _items.Add(newItem);
                SyncBack();
                RefreshListBox();
                ItemsListBox.SelectedIndex = _items.Count - 1;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("AddFailed", ex.Message),
                    LocalizationManager.GetString("Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex < 0)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("SelectItemToDelete"),
                    LocalizationManager.GetString("Tip"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _items.RemoveAt(ItemsListBox.SelectedIndex);
            SyncBack();
            RefreshListBox();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex < 0)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("SelectItemToCopy"),
                    LocalizationManager.GetString("Tip"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                var selected = _items[ItemsListBox.SelectedIndex];
                var cloned = Helpers.DeepCloneHelper.DeepClone(selected);
                _items.Insert(ItemsListBox.SelectedIndex + 1, cloned);
                SyncBack();
                RefreshListBox();
                ItemsListBox.SelectedIndex = ItemsListBox.SelectedIndex + 1;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("CopyFailed", ex.Message),
                    LocalizationManager.GetString("Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex <= 0) return;
            int index = ItemsListBox.SelectedIndex;
            var item = _items[index];
            _items.RemoveAt(index);
            _items.Insert(index - 1, item);
            SyncBack();
            RefreshListBox();
            ItemsListBox.SelectedIndex = index - 1;
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex < 0 || ItemsListBox.SelectedIndex >= _items.Count - 1) return;
            int index = ItemsListBox.SelectedIndex;
            var item = _items[index];
            _items.RemoveAt(index);
            _items.Insert(index + 1, item);
            SyncBack();
            RefreshListBox();
            ItemsListBox.SelectedIndex = index + 1;
        }

        private object CreateDefaultInstance(Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor != null) return ctor.Invoke(null);
            if (type.IsValueType) return Activator.CreateInstance(type);
            throw new InvalidOperationException(LocalizationManager.GetString("NoDefaultConstructor", type.Name));
        }
    }
}
