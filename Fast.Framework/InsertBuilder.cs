using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fast.Framework.Extensions;
using Fast.Framework.Interfaces;
using Fast.Framework.Models;

namespace Fast.Framework
{

    /// <summary>
    /// 插入建造者实现类
    /// </summary>
    public class InsertBuilder : IInsertBuilder
    {

        /// <summary>
        /// 数据库类型
        /// </summary>
        private readonly DbType dbType;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        public InsertBuilder(DbType dbType)
        {
            this.dbType = dbType;
            EntityDbMapping = new EntityDbMapping();
            EntityDbMappings = new List<EntityDbMapping>();
            BatchSqls = new List<BatchSql>();
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
        /// 实体数据库映射
        /// </summary>
        public EntityDbMapping EntityDbMapping { get; set; }

        /// <summary>
        /// 实体列表数据库映射
        /// </summary>
        public List<EntityDbMapping> EntityDbMappings { get; set; }

        /// <summary>
        /// 是否批量
        /// </summary>
        public bool IsBatch { get; set; }

        /// <summary>
        /// 批量Sql
        /// </summary>
        public List<BatchSql> BatchSqls { get; private set; }

        /// <summary>
        /// 到Sql
        /// </summary>
        /// <returns></returns>
        public string ToSql()
        {
            var sb = new StringBuilder();
            var parameterSymbol = dbType.MappingParameterSymbol();
            var identifier = dbType.MappingIdentifier();
            if (IsBatch)
            {
                BatchSqls.Clear();//初始化

                var rowCount = 2000 / EntityDbMappings[0].EntityInfos.Count;//计算每次最多插入多少行

                var headerSql = $"INSERT INTO {identifier.Insert(1, TableName)}\t({string.Join(",", EntityDbMappings[0].EntityInfos.Select(s => $"{identifier.Insert(1, s.ColumnName)}"))}) VALUES";//头部Sql

                var count = EntityDbMappings.Count / rowCount;//计算头部Sql最多添加次数

                if (count == 0)
                {
                    count = 1;
                }

                if (count * rowCount < EntityDbMappings.Count)
                {
                    count++;//增加剩余次数
                }

                for (int i = 0; i < count; i++)
                {
                    var batchSql = new BatchSql();
                    var valueSqls = new List<string>();
                    var startIndex = (i * rowCount);
                    var endIndex = (i * rowCount) + rowCount;

                    var list = EntityDbMappings.Take(new Range(startIndex, endIndex)).ToList();

                    for (int j = 0; j < list.Count; j++)
                    {
                        var newList = list[j].EntityInfos;
                        valueSqls.Add($"({string.Join(",", newList.Select(s => $"{parameterSymbol}{s.Identity}"))})");
                        batchSql.DbParameters.Append(list[j].DbParameters);
                    }
                    batchSql.Sql = $"{headerSql}\r\n{string.Join(",\r\n", valueSqls)}";
                    BatchSqls.Add(batchSql);
                }
                sb.Append(string.Join(";\r\n", BatchSqls.Select(s => s.Sql)));
            }
            else
            {
                sb.Append($"INSERT INTO {identifier.Insert(1, TableName)}\r\n(\r\n{string.Join(",\r\n", EntityDbMapping.EntityInfos.Select(s => $"{identifier.Insert(1, s.ColumnName)}"))}\r\n)\r\nVALUES\r\n(\r\n{string.Join(",\r\n", EntityDbMapping.DbParameters.Keys.Select(s => $"{parameterSymbol}{s}"))}\r\n)");
            }
            var sql = sb.ToString();
            return sql;
        }
    }
}
