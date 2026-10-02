using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using PropertyGridLib;
using PropertyGridLib.Controls;
using PropertyGridLib.Localization;
using HandyControl.Controls;

namespace PropertyGridLib.Dialogs
{
    public partial class CollectionEditorDialog
    {
        private IList _originalCollection;
        private List<object> _items;
        private Type _itemType;
        private readonly bool _isReadOnly;

        /// <summary>
        /// 集合编辑器。isReadOnly=true 时进入只读查看模式：
        /// 隐藏 增/删/复制/上下移/确定 功能按钮，仅可浏览内容（属性页仍可查看）。
        /// </summary>
        public CollectionEditorDialog(IList collection, bool isReadOnly = false)
        {
            // 先于 InitializeComponent 赋字体：首帧渲染即为全局字体，避免先按默认值绘制再跳变
            this.FontFamily = PropertyGrid.GlobalFontFamily;
            this.FontSize = PropertyGrid.GlobalFontSize;
            _isReadOnly = isReadOnly;
            InitializeComponent();
            UiScaler.Attach(this); // 分辨率/DPI 自适应（跟随宿主全局缩放因子）
            _originalCollection = collection;
            _itemType = GetItemType(collection);
            LoadItems(collection);
            if (_isReadOnly)
                EnterReadOnlyMode();
        }

        /// <summary>只读查看模式：隐藏功能按钮，仅可浏览；
        /// 内嵌属性页走 PropertyGrid.ReadOnly（编辑器全只读、重置禁用），可看不可改</summary>
        private void EnterReadOnlyMode()
        {
            AddButton.Visibility = Visibility.Collapsed;
            CopyButton.Visibility = Visibility.Collapsed;
            RemoveButton.Visibility = Visibility.Collapsed;
            MoveUpButton.Visibility = Visibility.Collapsed;
            MoveDownButton.Visibility = Visibility.Collapsed;
            OkButton.Visibility = Visibility.Collapsed;
            ItemPropertyGrid.ReadOnly = true;
            // 仅保留"关闭"（原取消按钮），点击直接关闭不保存
            CancelButton.Content = LocalizationManager.GetString("Close");
        }

        private Type GetItemType(IList collection)
        {
            if (collection == null)
                return typeof(object);

            // 优先从泛型参数获取类型（即使集合为空也能获取）
            var type = collection.GetType();
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length > 0)
                    return genericArgs[0];
            }

            // 从第一个元素获取类型
            if (collection.Count > 0)
                return collection[0]?.GetType() ?? typeof(object);

            return typeof(object);
        }

        private void LoadItems(IList collection)
        {
            _items = new List<object>();
            for (int i = 0; i < collection.Count; i++)
            {
                _items.Add(collection[i]);
            }
            RefreshListBox();
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
                ItemsListBox.SelectedIndex = items.Count-1;
        }

        private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsListBox.SelectedItem is ListBoxItemData data)
            {
                ItemPropertyGrid.SelectedObject = data.Value;
            }
            else
            {
                ItemPropertyGrid.SelectedObject = null;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newItem = CreateDefaultInstance(_itemType);
                _items.Add(newItem);
                RefreshListBox();
                ItemsListBox.SelectedIndex = _items.Count - 1;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(LocalizationManager.GetString("AddFailed", ex.Message),
                    LocalizationManager.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex < 0)
            {
                HandyControl.Controls.MessageBox.Show(LocalizationManager.GetString("SelectItemToDelete"),
                    LocalizationManager.GetString("Tip"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _items.RemoveAt(ItemsListBox.SelectedIndex);
            RefreshListBox();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex < 0)
            {
                HandyControl.Controls.MessageBox.Show(LocalizationManager.GetString("SelectItemToCopy"),
                    LocalizationManager.GetString("Tip"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var selectedItem = _items[ItemsListBox.SelectedIndex];
                var clonedItem = DeepClone(selectedItem);
                _items.Insert(ItemsListBox.SelectedIndex + 1, clonedItem);
                RefreshListBox();
                ItemsListBox.SelectedIndex = ItemsListBox.SelectedIndex + 1;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(LocalizationManager.GetString("CopyFailed", ex.Message),
                    LocalizationManager.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex <= 0)
                return;

            int index = ItemsListBox.SelectedIndex;
            var item = _items[index];
            _items.RemoveAt(index);
            _items.Insert(index - 1, item);
            RefreshListBox();
            ItemsListBox.SelectedIndex = index - 1;
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedIndex < 0 || ItemsListBox.SelectedIndex >= _items.Count - 1)
                return;

            int index = ItemsListBox.SelectedIndex;
            var item = _items[index];
            _items.RemoveAt(index);
            _items.Insert(index + 1, item);
            RefreshListBox();
            ItemsListBox.SelectedIndex = index + 1;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 将修改后的集合写回原集合
            _originalCollection.Clear();
            foreach (var item in _items)
            {
                _originalCollection.Add(item);
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private object CreateDefaultInstance(Type type)
        {
            // 尝试无参构造函数
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
                return constructor.Invoke(null);

            // 如果是值类型，返回默认值
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            throw new InvalidOperationException(LocalizationManager.GetString("NoDefaultConstructor", type.Name));
        }

        private object DeepClone(object obj)
        {
            return PropertyGridLib.Helpers.DeepCloneHelper.DeepClone(obj);
        }
    }

    public class ListBoxItemData
    {
        public int Index { get; set; }
        public string DisplayText { get; set; }
        public object Value { get; set; }
    }
}
