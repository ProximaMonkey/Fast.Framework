using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Fast.Framework.Models;


namespace Fast.Framework.Extensions
{

    /// <summary>
    /// 成员信息扩展类
    /// </summary>
    public static class MemberInfoExExtensions
    {

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="memberInfos">成员信息</param>
        /// <param name="compilerVar">编译器变量值</param>
        /// <param name="cacheKey">缓存密钥</param>
        /// <returns></returns>
        public static object GetValue(this Stack<MemberInfoEx> memberInfos, object compilerVar, out string cacheKey)
        {
            cacheKey = Convert.ToString(compilerVar);
            foreach (var item in memberInfos)
            {
                cacheKey += $".{item.Member.Name}_{string.Join("_", item.ArrayIndex)}";
                if (item.Member.MemberType == MemberTypes.Field)
                {
                    var fieldInfo = (FieldInfo)item.Member;
                    compilerVar = fieldInfo.GetValue(compilerVar);
                }
                if (item.Member.MemberType == MemberTypes.Property)
                {
                    var propertyInfo = (PropertyInfo)item.Member;
                    compilerVar = propertyInfo.GetValue(compilerVar);
                }
                if (item.ArrayIndex != null && item.ArrayIndex.Count > 0)
                {
                    var indexList = item.ArrayIndex.ToList();
                    foreach (var index in indexList)
                    {
                        compilerVar = (compilerVar as Array).GetValue(index);
                    }
                }
            }
            return compilerVar;
        }
    }
}

