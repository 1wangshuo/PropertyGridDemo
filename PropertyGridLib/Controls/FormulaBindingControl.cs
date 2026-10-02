using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 公式绑定控件：链接图标按钮 + 公式文本框 + Popup(TreeView)
    /// 放置在原值编辑器下方，不影响原值输入。
    /// 
    /// 独立用法（不依赖 PropertyGrid）：
    ///   &lt;controls:FormulaBindingControl
    ///       TreeItemsSource="{Binding MyTreeNodes}"
    ///       FormulaString="{Binding MyFormula, Mode=TwoWay}" /&gt;
    /// 或通过 FormulaTreeProvider 属性提供数据。
    /// </summary>
    public class FormulaBindingControl : Control
    {
        static FormulaBindingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormulaBindingControl),
                new FrameworkPropertyMetadata(typeof(FormulaBindingControl)));
        }

        public static readonly DependencyProperty FormulaStringProperty =
            DependencyProperty.Register(nameof(FormulaString), typeof(string), typeof(FormulaBindingControl),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(FormulaBindingControl),
                new PropertyMetadata(false));

        /// <summary>
        /// 树节点数据源（独立使用时直接设置，优先级最高）。
        /// 类型应为 IEnumerable&lt;FormulaTreeNode&gt;。
        /// </summary>
        public static readonly DependencyProperty TreeItemsSourceProperty =
            DependencyProperty.Register(nameof(TreeItemsSource), typeof(IEnumerable), typeof(FormulaBindingControl),
                new PropertyMetadata(null));

        /// <summary>
        /// 公式树数据提供者（独立使用时设置，优先级次于 TreeItemsSource）。
        /// </summary>
        public static readonly DependencyProperty FormulaTreeProviderProperty =
            DependencyProperty.Register(nameof(FormulaTreeProvider), typeof(IFormulaTreeProvider), typeof(FormulaBindingControl),
                new PropertyMetadata(null));

        /// <summary>
        /// 紧凑模式（默认 false = 通用完整样式，维持旧观感）。
        /// PropertyGrid / PropertyGridPro 行内覆盖式公式条设为 true：输入框小一号、
        /// 占位文字简化、弹树按钮更紧凑，与属性行视觉融合。
        /// 默认 false 保持现状，避免已使用项目的宿主一更新观感突变。
        /// </summary>
        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register(nameof(Compact), typeof(bool), typeof(FormulaBindingControl),
                new PropertyMetadata(false));

        /// <summary>公式字符串（双向绑定）</summary>
        public string FormulaString
        {
            get => (string)GetValue(FormulaStringProperty);
            set => SetValue(FormulaStringProperty, value);
        }

        /// <summary>是否只读</summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>树节点数据（独立使用时直接赋值）</summary>
        public IEnumerable TreeItemsSource
        {
            get => (IEnumerable)GetValue(TreeItemsSourceProperty);
            set => SetValue(TreeItemsSourceProperty, value);
        }

        /// <summary>公式树数据提供者</summary>
        public IFormulaTreeProvider FormulaTreeProvider
        {
            get => (IFormulaTreeProvider)GetValue(FormulaTreeProviderProperty);
            set => SetValue(FormulaTreeProviderProperty, value);
        }

        /// <summary>紧凑模式（默认 false = 通用完整样式）</summary>
        public bool Compact
        {
            get => (bool)GetValue(CompactProperty);
            set => SetValue(CompactProperty, value);
        }

        private Popup _popup;
        private TreeView _treeView;
        private ToggleButton _popupButton;
        private bool _treeLoaded;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _popup = GetTemplateChild("PART_Popup") as Popup;
            _treeView = GetTemplateChild("PART_TreeView") as TreeView;
            _popupButton = GetTemplateChild("PART_PopupButton") as ToggleButton;

            if (_popupButton != null)
            {
                _popupButton.Checked += PopupButton_Checked;
            }

            if (_treeView != null)
            {
                _treeView.MouseDoubleClick += TreeView_MouseDoubleClick;
                _treeView.KeyDown += TreeView_KeyDown;
            }
        }

        private void PopupButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!_treeLoaded)
            {
                LoadTreeItems();
                _treeLoaded = true;
            }
        }

        private void LoadTreeItems()
        {
            // 1) 优先使用独立设置的 TreeItemsSource
            if (TreeItemsSource != null)
            {
                if (_treeView != null)
                    _treeView.ItemsSource = TreeItemsSource;
                return;
            }

            // 2) 其次使用独立设置的 FormulaTreeProvider
            if (FormulaTreeProvider != null)
            {
                var items = FormulaTreeProvider.GetFormulaTree(null);
                if (_treeView != null)
                    _treeView.ItemsSource = items;
                return;
            }

            // 3) 回退：从 PropertyItem DataContext 获取（PropertyGrid 内嵌用法）
            if (DataContext is PropertyItem propertyItem)
            {
                var provider = propertyItem.FormulaTreeProvider;
                if (provider != null)
                {
                    var items = provider.GetFormulaTree(propertyItem);
                    if (_treeView != null)
                        _treeView.ItemsSource = items;
                }
            }
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectAndClose();
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectAndClose();
            }
        }

        private void SelectAndClose()
        {
            if (_treeView?.SelectedItem is FormulaTreeNode node && !string.IsNullOrEmpty(node.Formula))
            {
                FormulaString = node.Formula;
                if (_popupButton != null)
                    _popupButton.IsChecked = false;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T result)
                    return result;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
