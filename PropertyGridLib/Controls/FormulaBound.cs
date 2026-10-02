using System;
using System.ComponentModel;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 公式绑定类型，包装一个 T 和一个公式字符串。
    /// 用法：public FormulaBound&lt;string&gt; Name { get; set; } = new FormulaBound&lt;string&gt;();
    /// 配合 [FormulaEditor(typeof(xxxProvider))] 特性使用。
    /// </summary>
    [Serializable]
    public class FormulaBound<T> : INotifyPropertyChanged
    {
        private T _value;
        private string _formula = string.Empty;

        /// <summary>原始值</summary>
        public T Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>绑定公式，格式如 &{流程.任务.属性名}</summary>
        public string Formula
        {
            get => _formula;
            set
            {
                if (_formula != value)
                {
                    _formula = value;
                    OnPropertyChanged(nameof(Formula));
                }
            }
        }

        public FormulaBound() { }

        public FormulaBound(T value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value?.ToString() ?? string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
