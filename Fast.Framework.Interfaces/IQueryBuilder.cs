using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Interfaces
{

    /// <summary>
    /// 查询建造接口类
    /// </summary>
    public interface IQueryBuilder : ISqlBuilder
    {

        /// <summary>
        /// 去重
        /// </summary>
        bool Distinct { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        string Columns { get; set; }

        /// <summary>
        /// 连接
        /// </summary>
        List<string> Join { get; }

        /// <summary>
        /// 条件
        /// </summary>
        List<string> Where { get; }

        /// <summary>
        /// 数据库参数
        /// </summary>
        Dictionary<string, object> DbParameters { get; }

        /// <summary>
        /// 分组
        /// </summary>
        List<string> GroupBy { get; }

        /// <summary>
        /// 有
        /// </summary>
        List<string> Having { get; }

        /// <summary>
        /// 排序
        /// </summary>
        List<string> OrderBy { get; }

        /// <summary>
        /// 联合
        /// </summary>
        string Union { get; set; }

        /// <summary>
        /// 插入表名称
        /// </summary>
        string InsertTableName { get; set; }

        /// <summary>
        /// 插入列
        /// </summary>
        string InsertColumns { get; set; }
    }
}
