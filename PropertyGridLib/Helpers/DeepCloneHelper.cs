using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace PropertyGridLib.Helpers
{
    /// <summary>
    /// 深拷贝辅助类，使用 JSON 序列化替代已弃用的 BinaryFormatter
    /// </summary>
    internal static class DeepCloneHelper
    {
#if NET6_0_OR_GREATER
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            // 包含字段（不仅仅是属性）
            IncludeFields = true,
            // 允许循环引用
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            // 写入缩进（可选，用于调试）
            WriteIndented = false,
            // 默认忽略只读属性
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };
#else
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            // 写入缩进（可选，用于调试）
            WriteIndented = false
        };
#endif

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        public static object? DeepClone(object? obj)
        {
            if (obj == null) return null;

            var type = obj.GetType();

            // 值类型和字符串直接返回
            if (type.IsValueType || type == typeof(string))
                return obj;

            try
            {
                // 尝试使用 JSON 序列化
                var json = JsonSerializer.Serialize(obj, type, JsonOptions);
                return JsonSerializer.Deserialize(json, type, JsonOptions);
            }
            catch
            {
                // JSON 序列化失败，尝试使用反射浅拷贝
                try
                {
                    return ShallowCloneWithReflection(obj);
                }
                catch
                {
                    // 如果都失败，返回原始引用（Reset 功能受限但不崩溃）
                    return obj;
                }
            }
        }

        /// <summary>
        /// 使用反射进行浅拷贝（作为备用方案）
        /// </summary>
        private static object? ShallowCloneWithReflection(object obj)
        {
            if (obj == null) return null;

            var type = obj.GetType();

            // 尝试调用 MemberwiseClone
            var cloneMethod = type.GetMethod("MemberwiseClone", 
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (cloneMethod != null)
            {
                return cloneMethod.Invoke(obj, null);
            }

            // 如果没有 MemberwiseClone，尝试创建新实例并复制公共属性
            var newObj = Activator.CreateInstance(type);
            if (newObj == null) return obj;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    try
                    {
                        var value = prop.GetValue(obj);
                        prop.SetValue(newObj, value);
                    }
                    catch
                    {
                        // 忽略无法复制的属性
                    }
                }
            }

            return newObj;
        }
    }
}
