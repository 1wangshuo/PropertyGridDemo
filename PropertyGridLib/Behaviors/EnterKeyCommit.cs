using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HandyControl.Controls;
using TextBox = System.Windows.Controls.TextBox;

namespace PropertyGridLib.Behaviors
{
    /// <summary>
    /// 输入框回车提交：按 Enter 立即把当前文本写回绑定源，并移走焦点（等同失焦提交）。
    /// Shift/Ctrl/Alt + Enter 不拦截，留给多行控件插入换行。
    /// 用法：&lt;TextBox behaviors:EnterKeyCommit.IsEnabled="True" /&gt;
    /// </summary>
    public static class EnterKeyCommit
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(EnterKeyCommit),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Control control) return;

            // handledEventsToo=true：数字框等控件可能先在类处理器里吃掉回车，
            // 这里仍要收到并负责把焦点移走
            if ((bool)e.NewValue)
                control.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
            else
                control.RemoveHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
            if (sender is not Control control) return;

            // Shift/Ctrl/Alt + 回车 = 换行：不提交、不抢焦点，交给多行控件自己插入换行
            if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.None)
                return;

            // 先吃掉回车：单行框不插入换行，也不触发窗口默认按钮（如弹窗里的「确定」）
            e.Handled = true;

            // 文本类编辑器的绑定多为 LostFocus：主动提交一次，确保焦点移走前值已写回
            if (control is TextBox textBox)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                MoveFocusOut(textBox, control);
                return;
            }

            // 数字框的键盘焦点在模板内部的 TextBox 上：从真正持有焦点的元素移走，
            // 才会触发内部文本提交；若焦点只挪到数字框自己的上下按钮，则直接清焦点。
            var focused = Keyboard.FocusedElement as UIElement;
            var from = focused != null && (ReferenceEquals(focused, control) || control.IsAncestorOf(focused))
                ? focused
                : control;
            MoveFocusOut(from, control);

            // 内部 TextBox 失焦后数字框的 Value 才更新，下一轮再主动把外层绑定写回
            if (control is NumericUpDown numeric)
            {
                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    numeric.GetBindingExpression(NumericUpDown.ValueProperty)?.UpdateSource();
                }), DispatcherPriority.Input);
            }
        }

        private static void MoveFocusOut(UIElement from, Control control)
        {
            if (!from.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next))
                || control.IsKeyboardFocusWithin)
                Keyboard.ClearFocus();
        }
    }
}
