using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Models
{

    /// <summary>
    /// 解析Sql选项
    /// </summary>
    public class ResolveSqlOptions
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// 解析类型
        /// </summary>
        public ResolveSqlType ResolveSqlType { get; set; }

        /// <summary>
        /// 忽略参数
        /// </summary>
        public bool IgnoreParameter { get; set; }

        /// <summary>
        /// 忽略标识符
        /// </summary>
        public bool IgnoreIdentifier { get; set; }
    }
}
