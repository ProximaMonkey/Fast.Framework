using System;
using System.Collections.Generic;
using Fast.Framework.Models;

namespace Fast.Framework.Interfaces
{

    /// <summary>
    /// 更新建造者接口类
    /// </summary>
    public interface IUpdateBuilder : ISqlBuilder
    {
        /// <summary>
        /// 实体数据库映射
        /// </summary>
        EntityDbMapping EntityDbMapping { get; set; }

        /// <summary>
        /// 实体列表数据库映射
        /// </summary>
        List<EntityDbMapping> EntityDbMappings { get; set; }

        /// <summary>
        /// 条件
        /// </summary>
        List<string> Where { get; }

        /// <summary>
        /// 是否批量
        /// </summary>
        bool IsBatch { get; set; }

        /// <summary>
        /// 批量Sql
        /// </summary>
        List<BatchSql> BatchSqls { get; }
    }
}

