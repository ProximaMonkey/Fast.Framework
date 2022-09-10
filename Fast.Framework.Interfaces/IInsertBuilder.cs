using Fast.Framework.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Interfaces
{
    public interface IInsertBuilder : ISqlBuilder
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
        /// 是否批量
        /// </summary>
        bool IsBatch { get; set; }

        /// <summary>
        /// 批量Sql
        /// </summary>
        List<BatchSql> BatchSqls { get; }
    }
}
