using System;
using System.Reflection;


namespace Fast.Framework.Models
{

    /// <summary>
    /// 实体信息
    /// </summary>
    public class EntityInfo
    {
        /// <summary>
        /// 身份
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 列名称
        /// </summary>
        public string ColumnName { get; set; }
    }
}

