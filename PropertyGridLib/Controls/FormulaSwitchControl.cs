using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 「普通输入 ↔ 公式绑定」切换控件：单行值输入 + 绑定切换按钮，点击后在
    /// 普通文本输入与公式绑定条（<see cref="FormulaBindingControl"/>：公式输入 + 公式树选择）之间切换。
    /// 可脱离 PropertyGrid 独立使用，也是属性行「普通值/公式绑定」覆盖式编辑的同一交互模型。
    /// </summary>
    public class FormulaSwitchControl : Control
    {
        static FormulaSwitchControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormulaSwitchControl),
                new FrameworkPropertyMetadata(typeof(FormulaSwitchControl)));
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // 进入/退出公式模式的两个按钮共用同一处理（都翻转 IsFormulaMode）
            if (GetTemplateChild("PART_SwitchButton") is Button switchBtn)
            {
                switchBtn.Click -= SwitchButton_Click;
                switchBtn.Click += SwitchButton_Click;
            }
            if (GetTemplateChild("PART_CloseButton") is Button closeBtn)
            {
                closeBtn.Click -= SwitchButton_Click;
                closeBtn.Click += SwitchButton_Click;
            }
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            // 宿主替换了 SwitchCommand 则交给宿主；否则执行内置的公式模式翻转
            if (SwitchCommand is ICommand c)
            {
                if (c.CanExecute(null)) c.Execute(null);
                return;
            }
            ToggleFormulaMode();
        }

        // ---------------- 普通值 ----------------

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(FormulaSwitchControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>普通模式的值（双向）</summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // ---------------- 公式绑定 ----------------

        public static readonly DependencyProperty FormulaStringProperty =
            DependencyProperty.Register(nameof(FormulaString), typeof(string), typeof(FormulaSwitchControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnFormulaStringChanged));

        /// <summary>
        /// 公式一开始就有内容（宿主已绑定）时，默认切到公式模式 —— 打开即是「已经绑定」的状态，
        /// 而不是停在普通输入框上。只在首次由「无内容 → 有内容」补默认值：
        /// 之后完全交回宿主/用户（用户手动切回普通输入后不会被反复拉回公式模式）。
        /// </summary>
        private static void OnFormulaStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FormulaSwitchControl control) control.ApplyAutoFormulaMode();
        }

        private bool _autoFormulaModeApplied;

        private void ApplyAutoFormulaMode()
        {
            if (_autoFormulaModeApplied || string.IsNullOrEmpty(FormulaString)) return;
            _autoFormulaModeApplied = true;
            IsFormulaMode = true;
        }

        /// <summary>公式字符串（双向；如 "&amp;{Global,SysTime,Value}"）</summary>
        public string FormulaString
        {
            get => (string)GetValue(FormulaStringProperty);
            set => SetValue(FormulaStringProperty, value);
        }

        /// <summary>公式树数据源（直接绑定 FormulaTreeNode 集合；与 TreeProvider 二选一）</summary>
        public static readonly DependencyProperty TreeItemsSourceProperty =
            DependencyProperty.Register(nameof(TreeItemsSource), typeof(object), typeof(FormulaSwitchControl),
                new PropertyMetadata(null));

        public object TreeItemsSource
        {
            get => GetValue(TreeItemsSourceProperty);
            set => SetValue(TreeItemsSourceProperty, value);
        }

        /// <summary>公式树提供者（设置后控件自动调用 GetFormulaTree() 取树节点；与 TreeItemsSource 二选一）</summary>
        public static readonly DependencyProperty FormulaTreeProviderProperty =
            DependencyProperty.Register(nameof(FormulaTreeProvider), typeof(IFormulaTreeProvider), typeof(FormulaSwitchControl),
                new PropertyMetadata(null));

        public IFormulaTreeProvider FormulaTreeProvider
        {
            get => (IFormulaTreeProvider)GetValue(FormulaTreeProviderProperty);
            set => SetValue(FormulaTreeProviderProperty, value);
        }

        // ---------------- 状态 ----------------

        /// <summary>true = 公式绑定条；false = 普通输入。由绑定/关闭按钮切换</summary>
        public static readonly DependencyProperty IsFormulaModeProperty =
            DependencyProperty.Register(nameof(IsFormulaMode), typeof(bool), typeof(FormulaSwitchControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsFormulaMode
        {
            get => (bool)GetValue(IsFormulaModeProperty);
            set => SetValue(IsFormulaModeProperty, value);
        }

        /// <summary>只读：普通输入框只读 + 切换按钮禁用</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(FormulaSwitchControl),
                new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>紧凑模式（公式条输入框更小，用于行内覆盖布局）</summary>
        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register(nameof(Compact), typeof(bool), typeof(FormulaSwitchControl),
                new PropertyMetadata(false));

        public bool Compact
        {
            get => (bool)GetValue(CompactProperty);
            set => SetValue(CompactProperty, value);
        }

        /// <summary>普通输入框占位提示（可空）</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(FormulaSwitchControl),
                new PropertyMetadata(null));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>切换命令：替换后进入/退出公式模式时执行宿主命令，而非内置翻转</summary>
        public static readonly DependencyProperty SwitchCommandProperty =
            DependencyProperty.Register(nameof(SwitchCommand), typeof(ICommand), typeof(FormulaSwitchControl),
                new PropertyMetadata(null));

        /// <summary>切换命令（模板绑定；宿主可替换，null 时执行内置的公式模式翻转）</summary>
        public ICommand SwitchCommand
        {
            get => (ICommand)GetValue(SwitchCommandProperty);
            set => SetValue(SwitchCommandProperty, value);
        }

        /// <summary>内置切换：翻转 IsFormulaMode（宿主未替换 SwitchCommand 时执行）</summary>
        public void ToggleFormulaMode() => IsFormulaMode = !IsFormulaMode;
    }
}
