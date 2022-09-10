using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Models
{

    /// <summary>
    /// 解析Sql类型
    /// </summary>
    public enum ResolveSqlType
    {
        /// <summary>
        /// 条件
        /// </summary>
        Where = 0,

        /// <summary>
        /// 选择
        /// </summary>
        Select = 1,

        /// <summary>
        /// 分组
        /// </summary>
        GroupBy = 2,

        /// <summary>
        /// 排序
        /// </summary>
        OrderBy = 3,

        /// <summary>
        /// 列
        /// </summary>
        Column = 4
    }
}
