using System;
using System.Collections.Generic;


namespace Fast.Framework.Models
{

    /// <summary>
    /// 实体数据库映射
    /// </summary>
    public class EntityDbMapping
    {
        /// <summary>
        /// 实体信息
        /// </summary>
        public List<EntityInfo> EntityInfos { get; set; } = new List<EntityInfo>();

        /// <summary>
        /// 数据库参数
        /// </summary>
        public Dictionary<string, object> DbParameters { get; set; } = new Dictionary<string, object>();
    }
}

