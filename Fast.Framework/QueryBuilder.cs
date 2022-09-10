using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fast.Framework.Extensions;
using Fast.Framework.Interfaces;
using Fast.Framework.Models;

namespace Fast.Framework
{

    /// <summary>
    /// 查询构造
    /// </summary>
    public class QueryBuilder : IQueryBuilder
    {

        /// <summary>
        /// 数据库类型
        /// </summary>
        private readonly DbType dbType;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        public QueryBuilder(DbType dbType)
        {
            this.dbType = dbType;
            Join = new List<string>();
            Where = new List<string>();
            DbParameters = new Dictionary<string, object>();
            GroupBy = new List<string>();
            Having = new List<string>();
            OrderBy = new List<string>();
        }

        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 去重
        /// </summary>
        public bool Distinct { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        public string Columns { get; set; }

        /// <summary>
        /// 连接
        /// </summary>
        public List<string> Join { get; }

        /// <summary>
        /// 条件
        /// </summary>
        public List<string> Where { get; }

        /// <summary>
        /// 数据库参数
        /// </summary>
        public Dictionary<string, object> DbParameters { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        public List<string> GroupBy { get; }

        /// <summary>
        /// 有
        /// </summary>
        public List<string> Having { get; }

        /// <summary>
        /// 排序
        /// </summary>
        public List<string> OrderBy { get; }

        /// <summary>
        /// 联合
        /// </summary>
        public string Union { get; set; }

        /// <summary>
        /// 插入表名称
        /// </summary>
        public string InsertTableName { get; set; }

        /// <summary>
        /// 插入列
        /// </summary>
        public string InsertColumns { get; set; }

        /// <summary>
        /// 到Sql
        /// </summary>
        /// <returns></returns>
        public string ToSql()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(InsertTableName))
            {
                sb.Append($"INSERT INTO {dbType.MappingIdentifier().Insert(1, InsertTableName)} ( {InsertColumns} ) \r\n");
            }
            sb.Append($"SELECT ");
            if (Distinct)
            {
                sb.Append("DISTINCT ");
            }
            sb.Append($"{(string.IsNullOrWhiteSpace(Columns) ? "*" : Columns)} FROM ");
            if (!string.IsNullOrWhiteSpace(Union))
            {
                sb.Append("\r\n(\r\n");
                sb.AppendLine(string.Join("\r\nUNION ALL\r\n", Union));
                sb.Append(") ");
            }
            sb.Append($"{dbType.MappingIdentifier().Insert(1, TableName)} ");
            if (!string.IsNullOrWhiteSpace(Alias))
            {
                sb.Append($"{Alias}");
            }
            if (Join.Count > 0)
            {
                sb.Append($"\r\n{string.Join("\r\n", Join)}");
            }
            if (Where.Count > 0)
            {
                sb.Append($"\r\nWHERE {string.Join(" AND ", Where)}");
            }
            if (GroupBy.Count > 0)
            {
                sb.Append($"\r\nGROUP BY {string.Join(",", GroupBy)}");
            }
            if (Having.Count > 0)
            {
                sb.Append($"\r\nHAVING {string.Join(" AND ", Having)}");
            }
            if (OrderBy.Count > 0)
            {
                sb.Append($"\r\nORDER BY {string.Join(",", OrderBy)}");
            }
            var sql = sb.ToString();
            return sql;
        }
    }
}
