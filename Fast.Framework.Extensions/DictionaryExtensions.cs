using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Extensions
{

    /// <summary>
    /// 键值对扩展类
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 身份
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="parameterIndex">参数索引</param>
        /// <returns></returns>
        public static Dictionary<string, object> Identity(this Dictionary<string, object> source, int parameterIndex = 0)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var item in source)
            {
                dictionary.Add($"{item.Key}_{parameterIndex}", item.Value);
            }
            return dictionary;
        }

        /// <summary>
        /// 追加
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="values">值</param>
        /// <returns></returns>
        public static Dictionary<string, object> Append(this Dictionary<string, object> source, Dictionary<string, object> values)
        {
            foreach (var item in values)
            {
                source.Add(item.Key, item.Value);
            }
            return source;
        }
    }
}
