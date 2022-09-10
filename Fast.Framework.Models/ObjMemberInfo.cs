using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fast.Framework.Models
{
    /// <summary>
    /// 对象成员信息
    /// </summary>
    public class ObjMemberInfo
    {
        /// <summary>
        /// 目标类型
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// 目标
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// 成员信息
        /// </summary>
        public Stack<MemberInfoEx> MemberInfos { get; set; }

        /// <summary>
        /// 模板
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public ObjMemberInfo()
        {
            MemberInfos = new Stack<MemberInfoEx>();
        }
    }
}

