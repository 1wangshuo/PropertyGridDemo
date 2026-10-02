using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    public partial class SimpleCollectionEditorControl : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IList), typeof(SimpleCollectionEditorControl),
                new PropertyMetadata(null, OnItemsChanged));

        public static readonly DependencyProperty ElementTypeProperty =
            DependencyProperty.Register(nameof(ElementType), typeof(Type), typeof(SimpleCollectionEditorControl),
                new PropertyMetadata(typeof(string)));

        private readonly List<object> _items = new List<object>();
        private Type _elementType;

        public IList Items
        {
            get => (IList)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public Type ElementType
        {
            get => (Type)GetValue(ElementTypeProperty);
            set => SetValue(ElementTypeProperty, value);
        }

        public SimpleCollectionEditorControl()
        {
            InitializeComponent();
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleCollectionEditorControl)d).LoadItems();
        }

        private void LoadItems()
        {
            _items.Clear();
            var collection = Items;
            if (collection == null)
            {
                _elementType = ElementType ?? typeof(string);
                ItemsList.ItemsSource = _items;
                return;
            }

            _elementType = DetectElementType(collection.GetType());
            ElementType = _elementType;

            foreach (var item in collection)
                _items.Add(item);

            ItemsList.ItemsSource = _items;
        }

        private Type DetectElementType(Type collectionType)
        {
            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments()[0];

            foreach (var iface in collectionType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IList<>))
                    return iface.GetGenericArguments()[0];
            }

            return typeof(string);
        }

        private void SyncBack()
        {
            var collection = Items;
            if (collection == null) return;
            collection.Clear();
            foreach (var item in _items)
                collection.Add(item);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AddItem();
        }

        private void AddItem()
        {
            var text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                var value = ConvertValue(text, _elementType);
                _items.Add(value);
                SyncBack();
                InputBox.Clear();
                ItemsList.Items.Refresh();
                ItemsList.SelectedIndex = ItemsList.Items.Count - 1;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("ConvertFailed", ex.Message),
                    LocalizationManager.GetString("Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
            return Convert.ChangeType(text, targetType);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsList.SelectedItem == null)
            {
                HandyControl.Controls.MessageBox.Show(
                    LocalizationManager.GetString("SelectItemToDelete"),
                    LocalizationManager.GetString("Tip"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _items.Remove(ItemsList.SelectedItem);
            SyncBack();
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
            SyncBack();
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
            SyncBack();
            ItemsList.SelectedIndex = index + 1;
            ItemsList.Items.Refresh();
        }
    }
}
