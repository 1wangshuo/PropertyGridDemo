using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 多选列表控件：显示已选项摘要 + 弹出 CheckBox 列表
    /// </summary>
    public class MultiSelectControl : Control
    {
        static MultiSelectControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectControl),
                new FrameworkPropertyMetadata(typeof(MultiSelectControl)));
        }

        #region 依赖属性

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IList<string>), typeof(MultiSelectControl),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemsChanged));

        public static readonly DependencyProperty AvailableItemsProperty =
            DependencyProperty.Register(nameof(AvailableItems), typeof(IList<string>), typeof(MultiSelectControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(MultiSelectControl),
                new PropertyMetadata(false));

        /// <summary>已选中的项列表（双向绑定到属性值）</summary>
        public IList<string> SelectedItems
        {
            get => (IList<string>)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        /// <summary>所有可选项</summary>
        public IList<string> AvailableItems
        {
            get => (IList<string>)GetValue(AvailableItemsProperty);
            set => SetValue(AvailableItemsProperty, value);
        }

        /// <summary>是否只读</summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>已选项摘要文本</summary>
        public string SummaryText
        {
            get => (string)GetValue(SummaryTextProperty);
            private set => SetValue(SummaryTextProperty, value);
        }

        public static readonly DependencyProperty SummaryTextProperty =
            DependencyProperty.Register(nameof(SummaryText), typeof(string), typeof(MultiSelectControl),
                new PropertyMetadata(""));

        #endregion

        private Popup _popup;
        private ToggleButton _popupButton;
        private ItemsControl _checkBoxList;
        private bool _itemsLoaded;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _popup = GetTemplateChild("PART_Popup") as Popup;
            _popupButton = GetTemplateChild("PART_PopupButton") as ToggleButton;
            _checkBoxList = GetTemplateChild("PART_CheckBoxList") as ItemsControl;

            if (_popupButton != null)
            {
                _popupButton.Checked += PopupButton_Checked;
                _popupButton.Unchecked += PopupButton_Unchecked;
            }
        }

        private void PopupButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!_itemsLoaded)
            {
                LoadAvailableItems();
                _itemsLoaded = true;
            }
            BuildCheckBoxes();
        }

        private void PopupButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // 关闭时同步已选项到属性值
            SyncSelectedItems();
        }

        private void LoadAvailableItems()
        {
            if (DataContext is PropertyItem propertyItem)
            {
                var provider = propertyItem.MultiSelectProvider;
                if (provider != null)
                {
                    var items = provider.GetAvailableItems(propertyItem);
                    AvailableItems = items ?? new List<string>();
                }
            }
        }

        private void BuildCheckBoxes()
        {
            if (_checkBoxList == null || AvailableItems == null) return;

            var selected = SelectedItems ?? new List<string>();

            var list = new List<CheckBoxItem>();
            foreach (var item in AvailableItems)
            {
                list.Add(new CheckBoxItem
                {
                    Text = item,
                    IsChecked = selected.Contains(item)
                });
            }
            _checkBoxList.ItemsSource = list;
        }

        private void SyncSelectedItems()
        {
            if (_checkBoxList?.ItemsSource is IEnumerable<CheckBoxItem> items)
            {
                var selected = items.Where(x => x.IsChecked).Select(x => x.Text).ToList();

                if (DataContext is PropertyItem propertyItem)
                {
                    // 直接写入属性值
                    propertyItem.Value = selected;
                }

                SelectedItems = selected;
                UpdateSummary(selected);
            }
        }

        private void UpdateSummary(IList<string> selected)
        {
            if (selected == null || selected.Count == 0)
            {
                SummaryText = LocalizationManager.GetString("NotSelected");
            }
            else if (selected.Count <= 3)
            {
                SummaryText = string.Join(", ", selected);
            }
            else
            {
                SummaryText = LocalizationManager.GetString("SelectedCount", selected.Count);
            }
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiSelectControl ctrl)
            {
                ctrl.UpdateSummary(e.NewValue as IList<string>);
            }
        }

        public class CheckBoxItem
        {
            public string Text { get; set; }
            public bool IsChecked { get; set; }
        }
    }
}
