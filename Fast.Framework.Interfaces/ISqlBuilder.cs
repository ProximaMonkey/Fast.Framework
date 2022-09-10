using Fast.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Interfaces
{

    /// <summary>
    /// Sql构建接口类
    /// </summary>
    public interface ISqlBuilder
    {
        /// <summary>
        /// 表名称
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// 到Sql
        /// </summary>
        /// <returns></returns>
        string ToSql();
    }
}
