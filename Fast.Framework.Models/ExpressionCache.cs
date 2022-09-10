using System;
using System.Reflection;
using System.Collections.Generic;


namespace Fast.Framework.Models
{

    /// <summary>
    /// 表达式缓存
    /// </summary>
    public class ExpressionCache<T>
    {
        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 成员信息缓存
        /// </summary>
        public Dictionary<string, ObjMemberInfo> MemberInfoCache { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public ExpressionCache()
        {
            MemberInfoCache = new Dictionary<string, ObjMemberInfo>();
        }
    }

}

