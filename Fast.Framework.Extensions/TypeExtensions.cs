using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Extensions
{

    /// <summary>
    /// 类型扩展类
    /// </summary>
    public static class TypeExtensions
    {

        /// <summary>
        /// 类型默认值缓存
        /// </summary>
        private static readonly Dictionary<Type, object> typeDefaultValueCache;

        /// <summary>
        /// 静态构造方法
        /// </summary>
        static TypeExtensions()
        {
            typeDefaultValueCache = new Dictionary<Type, object>()
            {
                { typeof(object), default(object)},
                { typeof(string), default(string)},
                { typeof(bool), default(bool)},
                { typeof(DateTime), default(DateTime)},
                { typeof(Guid), default(Guid)},
                { typeof(int), default(int)},
                { typeof(long), default(long)},
                { typeof(decimal), default(decimal)},
                { typeof(double), default(double)},
                { typeof(float), default(float)},
                { typeof(short), default(short)},
                { typeof(byte), default(byte)},
                { typeof(char), default(char)}
            };
        }

        /// <summary>
        /// 获取类型默认值
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object GetTypeDefaultValue(this Type type)
        {
            if (typeDefaultValueCache.ContainsKey(type))
            {
                return typeDefaultValueCache[type];
            }
            return null;
        }

        /// <summary>
        /// 获取表名称
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static string GetTableName(this Type type)
        {
            if (type.IsDefined(typeof(TableAttribute)))
            {
                return type.GetCustomAttribute<TableAttribute>().Name;
            }
            return type.Name;
        }
    }
}
