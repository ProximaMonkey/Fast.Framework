using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Models
{

    /// <summary>
    /// 批量Sql
    /// </summary>
    public class BatchSql
    {
        /// <summary>
        /// Sql
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// 数据库参数
        /// </summary>
        public Dictionary<string, object> DbParameters { get; set; } = new Dictionary<string, object>();
    }
}
