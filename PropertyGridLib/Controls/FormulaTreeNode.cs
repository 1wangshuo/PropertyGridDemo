using System;
using System.Collections.Generic;
using System.Linq;

namespace PropertyGridLib.Controls
{
    /// <summary>
    /// 公式绑定树节点数据
    /// </summary>
    public class FormulaTreeNode
    {
        /// <summary>显示文本</summary>
        public string Header { get; set; }

        /// <summary>选中后填入的公式字符串(仅叶子节点需要设置)</summary>
        public string Formula { get; set; }

        /// <summary>子节点</summary>
        public List<FormulaTreeNode> Children { get; set; } = new List<FormulaTreeNode>();

        /// <summary>
        /// 公式指向的目标值类型。宿主构建树时填充,UI 自动生成 " (int)" 风格提示。
        /// 非叶子节点(如分类容器)通常为 null,UI 不会显示类型提示。
        /// </summary>
        public Type FormulaType { get; set; }

        /// <summary>
        /// 类型提示文本,如 "(int)"、"(string)"、"(SampleObject2)"。
        /// 由 FormulaType 自动生成,也可由宿主直接赋自定义文本(覆盖默认生成)。
        /// 为 null 时不显示。
        /// </summary>
        public string TypeHint
        {
            get
            {
                if (_typeHint != null) return _typeHint;       // 宿主显式设置优先
                if (FormulaType == null) return null;           // 无类型则不提示
                return "(" + GetFriendlyTypeName(FormulaType) + ")";
            }
            set => _typeHint = value;
        }
        private string _typeHint;

        private static string GetFriendlyTypeName(Type type)
        {
            if (type == null) return "object";

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlying = Nullable.GetUnderlyingType(type);
                return GetFriendlyTypeName(underlying) + "?";
            }

            if (type.IsGenericType)
            {
                var name = type.Name;
                var tickIdx = name.IndexOf('`');
                if (tickIdx > 0) name = name.Substring(0, tickIdx);
                var args = type.GetGenericArguments();
                return name + "<" + string.Join(", ", args.Select(GetFriendlyTypeName)) + ">";
            }

            return type.Name;
        }
    }
}
