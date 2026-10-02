using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyGridLib;
using PropertyGridLib.Localization;
using HandyControl.Controls;

namespace PropertyGridLib.Dialogs
{
    public partial class SimpleCollectionEditorDialog 
    {
        private readonly IList _originalCollection;
        private readonly Type _elementType;
        private readonly List<object> _items;
        private readonly bool _isReadOnly;

        /// <summary>
        /// 简单集合编辑器。isReadOnly=true 时进入只读查看模式：
        /// 隐藏 输入框/增/删/上下移/确定 功能，仅可浏览列表内容。
        /// </summary>
        public SimpleCollectionEditorDialog(IList collection, bool isReadOnly = false)
        {
            // 先于 InitializeComponent 赋字体：首帧渲染即为全局字体，避免先按默认值绘制再跳变
            this.FontFamily = PropertyGrid.GlobalFontFamily;
            this.FontSize = PropertyGrid.GlobalFontSize;
            _isReadOnly = isReadOnly;
            InitializeComponent();
            UiScaler.Attach(this); // 分辨率/DPI 自适应（跟随宿主全局缩放因子）
            _originalCollection = collection;
            _elementType = GetElementType(collection.GetType());
            _items = new List<object>();

            // 复制集合内容
            foreach (var item in collection)
            {
                _items.Add(item);
            }

            ItemsList.ItemsSource = _items;
            if (_isReadOnly)
                EnterReadOnlyMode();
        }

        /// <summary>只读查看模式：隐藏功能按钮，仅可浏览</summary>
        private void EnterReadOnlyMode()
        {
            InputBox.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            MoveUpButton.Visibility = Visibility.Collapsed;
            MoveDownButton.Visibility = Visibility.Collapsed;
            OkButton.Visibility = Visibility.Collapsed;
            // 仅保留"关闭"（原取消按钮），点击直接关闭不保存
            CancelButton.Content = LocalizationManager.GetString("Close");
        }

        private Type GetElementType(Type collectionType)
        {
            // 尝试从泛型参数获取
            if (collectionType.IsGenericType)
            {
                return collectionType.GetGenericArguments()[0];
            }

            // 尝试从接口获取
            foreach (var iface in collectionType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return iface.GetGenericArguments()[0];
                }
            }

            // 默认返回 string
            return typeof(string);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddItem();
            }
        }

        private void AddItem()
        {
            var text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                var value = ConvertValue(text, _elementType);
                _items.Add(value);
                InputBox.Clear();
                ItemsList.Items.Refresh();
                ItemsList.SelectedIndex = ItemsList.Items.Count - 1;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(LocalizationManager.GetString("ConvertFailed", ex.Message),
                    LocalizationManager.GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private object ConvertValue(string text, Type targetType)
        {
            if (targetType == typeof(string)) return text;
            if (targetType == typeof(int)) return int.Parse(text);
            if (targetType == typeof(double)) return double.Parse(text);
            if (targetType == typeof(float)) return float.Parse(text);
            if (targetType == typeof(decimal)) return decimal.Parse(text);
            if (targetType == typeof(long)) return long.Parse(text);
            if (targetType == typeof(bool)) return bool.Parse(text);
            if (targetType == typeof(DateTime)) return DateTime.Parse(text);

            // 尝试使用 Convert.ChangeType
            return Convert.ChangeType(text, targetType);
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem == null)
            {
                HandyControl.Controls.MessageBox.Show(LocalizationManager.GetString("SelectItemToDelete"),
                    LocalizationManager.GetString("Tip"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _items.Remove(ItemsList.SelectedItem);
            ItemsList.Items.Refresh();
            ItemsList.SelectedIndex = ItemsList.Items.Count - 1;
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem == null) return;
            var index = ItemsList.SelectedIndex;
            if (index <= 0) return;

            var item = _items[index];
            _items.RemoveAt(index);
            _items.Insert(index - 1, item);
            ItemsList.SelectedIndex = index - 1;
            ItemsList.Items.Refresh();
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem == null) return;
            var index = ItemsList.SelectedIndex;
            if (index >= _items.Count - 1) return;

            var item = _items[index];
            _items.RemoveAt(index);
            _items.Insert(index + 1, item);
            ItemsList.SelectedIndex = index + 1;
            ItemsList.Items.Refresh();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 清空原集合
            _originalCollection.Clear();

            // 添加新内容
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
    }
}
