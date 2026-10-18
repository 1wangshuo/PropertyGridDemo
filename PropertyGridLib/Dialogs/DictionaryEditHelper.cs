using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using PropertyGridLib.Localization;

namespace PropertyGridLib.Dialogs
{
    /// <summary>字典条目视图模型（键 + 值 + 列表显示文本）</summary>
    public class DictEntry : INotifyPropertyChanged
    {
        private object _key;
        private object _value;

        public object Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(nameof(Key)); OnPropertyChanged(nameof(KeyDisplay)); }
        }

        public object Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); OnPropertyChanged(nameof(ValueDisplay)); }
        }

        public string KeyDisplay => _key?.ToString() ?? "(null)";
        public string ValueDisplay => _value?.ToString() ?? "(null)";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        /// <summary>
        /// 复杂键/值被内嵌属性页就地改过后，按当前对象重算显示文本并通知行更新。
        /// Key/Value 的 setter 只在「引用被替换」时才发通知；就地修改同一个对象不走 setter，
        /// 列表行便一直停在旧文本，必须由值变更事件显式触发这里。
        /// </summary>
        public void RaiseDisplayChanged()
        {
            OnPropertyChanged(nameof(KeyDisplay));
            OnPropertyChanged(nameof(ValueDisplay));
        }

        /// <summary>按当前对象重算所有条目的显示文本（供字典编辑器在内嵌属性页值变更后调用）</summary>
        public static void RefreshDisplayTexts(IEnumerable entries)
        {
            if (entries == null) return;
            foreach (var entry in entries)
                (entry as DictEntry)?.RaiseDisplayChanged();
        }
    }

    /// <summary>
    /// 字典编辑器共享逻辑：键值类型推断、简单/复杂判定、字符串转换、默认值创建、唯一键生成。
    /// 供 DictionaryEditorDialog（弹窗）与 DictionaryEditorControl（内联）复用。
    /// 键、值均可为简单类型（文本框编辑）或复杂类型（内嵌 PropertyGrid 编辑，支持继续嵌套）。
    /// </summary>
    internal static class DictionaryEditHelper
    {
        /// <summary>从字典类型推断键值类型（优先 IDictionary&lt;,&gt; 泛型参数）</summary>
        public static KeyValuePair<Type, Type> GetKeyValueTypes(Type dictType)
        {
            if (dictType.IsGenericType && dictType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var a = dictType.GetGenericArguments();
                return new KeyValuePair<Type, Type>(a[0], a[1]);
            }
            foreach (var iface in dictType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var a = iface.GetGenericArguments();
                    return new KeyValuePair<Type, Type>(a[0], a[1]);
                }
            }
            if (dictType.IsGenericType)
            {
                var a = dictType.GetGenericArguments();
                if (a.Length == 2) return new KeyValuePair<Type, Type>(a[0], a[1]);
            }
            return new KeyValuePair<Type, Type>(typeof(object), typeof(object));
        }

        /// <summary>是否为可直接用文本框编辑的简单类型</summary>
        public static bool IsSimpleType(Type type)
        {
            if (type == null || type == typeof(object)) return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return IsSimpleType(Nullable.GetUnderlyingType(type));
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
                || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid);
        }

        /// <summary>字符串转目标类型（借助 TypeConverter，支持基元/枚举/DateTime 等）</summary>
        public static object ConvertFromString(string text, Type targetType)
        {
            if (targetType == typeof(string) || targetType == typeof(object)) return text;

            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying != null)
            {
                if (string.IsNullOrWhiteSpace(text)) return null;
                targetType = underlying;
            }
            else if (string.IsNullOrWhiteSpace(text))
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
                return converter.ConvertFromString(text);
            return Convert.ChangeType(text, targetType);
        }

        /// <summary>创建类型默认值（简单类型给默认值，复杂类型尝试无参构造）</summary>
        public static object CreateDefaultValue(Type type)
        {
            if (type == typeof(string)) return string.Empty;
            if (IsSimpleType(type))
                return type != null && type.IsValueType ? Activator.CreateInstance(type) : null;
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor != null) return ctor.Invoke(null);
            return type != null && type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>生成一个尽量唯一的键；复杂键返回默认实例（唯一性在写回时统一校验）</summary>
        public static object MakeUniqueKey(IList<DictEntry> entries, Type keyType)
        {
            // 复杂键（key 是对象）：创建默认实例，交由用户在属性网格中编辑
            if (!IsSimpleType(keyType))
                return CreateDefaultValue(keyType);

            if (keyType == typeof(string) || keyType == typeof(object))
            {
                int n = entries.Count + 1;
                string key;
                do { key = "Key" + n; n++; } while (entries.Any(x => Equals(x.Key, key)));
                return key;
            }
            if (keyType == typeof(int) || keyType == typeof(long) || keyType == typeof(short)
                || keyType == typeof(byte) || keyType == typeof(uint) || keyType == typeof(ulong))
            {
                long max = 0;
                foreach (var x in entries)
                    if (x.Key != null && long.TryParse(x.Key.ToString(), out var v) && v > max) max = v;
                long candidate = max + 1;
                while (entries.Any(x => x.Key != null && long.TryParse(x.Key.ToString(), out var v) && v == candidate))
                    candidate++;
                return Convert.ChangeType(candidate, keyType);
            }
            var def = keyType.IsValueType ? Activator.CreateInstance(keyType) : null;
            if (entries.Any(x => Equals(x.Key, def)))
                throw new InvalidOperationException(LocalizationManager.GetString("DuplicateKey", def?.ToString() ?? "(null)"));
            return def;
        }

        /// <summary>是否存在重复键（写回前校验，避免 IDictionary.Add 抛异常）</summary>
        public static DictEntry FindDuplicateKey(IEnumerable<DictEntry> entries)
        {
            var dup = entries.GroupBy(x => x.Key).FirstOrDefault(g => g.Count() > 1);
            return dup?.First();
        }

        /// <summary>判断类型是否为字典（实现非泛型 IDictionary 或泛型 IDictionary&lt;,&gt;）</summary>
        public static bool IsDictionaryType(Type type)
        {
            if (type == null) return false;
            if (typeof(IDictionary).IsAssignableFrom(type)) return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)) return true;
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return true;
            }
            return false;
        }

        /// <summary>判断类型是否为非字典的列表（实现 IList，且不是 IDictionary）</summary>
        public static bool IsListType(Type type)
        {
            if (type == null) return false;
            if (IsDictionaryType(type)) return false;
            return typeof(IList).IsAssignableFrom(type);
        }

        /// <summary>获取集合元素类型（优先泛型参数，其次取首元素类型）</summary>
        public static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType == null) return typeof(object);
            if (collectionType.IsGenericType)
            {
                var args = collectionType.GetGenericArguments();
                if (args.Length > 0) return args[0];
            }
            return collectionType.GetElementType() ?? typeof(object);
        }

        /// <summary>获取集合/字典的摘要文本（"N 项"）</summary>
        public static string GetCollectionSummary(object value)
        {
            if (value == null) return string.Empty;
            if (value is IDictionary dict) return LocalizationManager.GetString("ItemsCount", dict.Count);
            if (value is ICollection col) return LocalizationManager.GetString("ItemsCount", col.Count);
            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 打开集合/字典编辑器弹窗（字典 → DictionaryEditorDialog，简单列表 → SimpleCollectionEditorDialog，复杂列表 → CollectionEditorDialog）。
        /// isReadOnly 会向下传递：嵌套字典/列表的只读外层弹窗，其内层弹窗同样只读（否则只读模型可被内层改写）。
        /// 返回 true 表示用户确认且集合已被就地修改。
        /// </summary>
        public static bool OpenCollectionEditor(Window owner, object collection, bool isReadOnly = false)
        {
            if (collection is IDictionary dictionary)
            {
                var dlg = new DictionaryEditorDialog(dictionary, isReadOnly);
                return ShowDialog(dlg, owner);
            }

            if (collection is IList list)
            {
                var elementType = GetCollectionElementType(list.GetType());
                if (list.Count > 0)
                    elementType = list[0]?.GetType() ?? elementType;

                var isSimple = IsSimpleType(elementType);
                var dlg = isSimple
                    ? (System.Windows.Window)new SimpleCollectionEditorDialog(list, isReadOnly)
                    : new CollectionEditorDialog(list, isReadOnly);
                return ShowDialog(dlg, owner);
            }

            return false;
        }

        private static bool ShowDialog(System.Windows.Window dlg, Window owner)
        {
            dlg.Owner = owner ?? System.Windows.Application.Current.MainWindow;
            return dlg.ShowDialog() == true;
        }
    }
}
