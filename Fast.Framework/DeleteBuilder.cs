using System;
using System.Collections.Generic;
using System.Text;
using Fast.Framework.Interfaces;
using Fast.Framework.Extensions;
using Fast.Framework.Models;

namespace Fast.Framework
{

    /// <summary>
    /// 删除建造者实现类
    /// </summary>
    public class DeleteBuilder : IDeleteBuilder
    {

        /// <summary>
        /// 数据库类型
        /// </summary>
        private readonly DbType dbType;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        public DeleteBuilder(DbType dbType)
        {
            this.dbType = dbType;
            DbParameters = new Dictionary<string, object>();
            Where = new List<string>();
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
        /// 数据库参数
        /// </summary>
        public Dictionary<string, object> DbParameters { get; set; }

        /// <summary>
        /// 条件
        /// </summary>
        public List<string> Where { get; }

        /// <summary>
        /// 到Sql
        /// </summary>
        /// <returns></returns>
        public string ToSql()
        {
            var sb = new StringBuilder();
            sb.Append($"DELETE FROM {dbType.MappingIdentifier().Insert(1, TableName)}");
            if (Where.Count > 0)
            {
                sb.Append($" WHERE {string.Join(" AND ", Where)}");
            }
            var sql = sb.ToString();
            return sql;
        }
    }
}

