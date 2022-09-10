using System;
using System.Collections.Generic;
using System.Text;
using Fast.Framework.Interfaces;
using Fast.Framework.Extensions;
using Fast.Framework.Models;
using System.Linq;

namespace Fast.Framework
{

    /// <summary>
    /// 更新建造者实现类
    /// </summary>
    public class UpdateBuilder : IUpdateBuilder
    {

        /// <summary>
        /// 数据库类型
        /// </summary>
        private readonly DbType dbType;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        public UpdateBuilder(DbType dbType)
        {
            this.dbType = dbType;
            EntityDbMapping = new EntityDbMapping();
            EntityDbMappings = new List<EntityDbMapping>();
            Where = new List<string>();
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
        /// 条件
        /// </summary>
        public List<string> Where { get; }

        /// <summary>
        /// 是否批量
        /// </summary>
        public bool IsBatch { get; set; }

        /// <summary>
        /// 批量Sql
        /// </summary>
        public List<BatchSql> BatchSqls { get; }

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

                var count = EntityDbMappings.Count / rowCount;//计算Sql条数

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
                    var sqls = new List<string>();
                    var startIndex = (i * rowCount);
                    var endIndex = (i * rowCount) + rowCount;

                    var list = EntityDbMappings.Take(new Range(startIndex, endIndex)).ToList();

                    for (int j = 0; j < list.Count; j++)
                    {
                        var newList = list[j].EntityInfos;
                        var updateSql = $"UPDATE {identifier.Insert(1, TableName)} SET {string.Join(",", newList.Where(w => !w.IsPrimaryKey).Select(s => $"{identifier.Insert(1, s.ColumnName)} = {parameterSymbol}{s.Identity}"))}";

                        var entityDbMapping = newList.FirstOrDefault(f => f.IsPrimaryKey);
                        if (entityDbMapping == null)
                        {
                            throw new Exception("无更新条件列,请使用KeyAuttribue特性标记属性或使用WhereColumns方法显示指定更新列.");
                        }
                        else
                        {
                            updateSql += $" WHERE {entityDbMapping.ColumnName} = {parameterSymbol}{entityDbMapping.Identity} ";
                        }
                        sqls.Add(updateSql);
                        batchSql.DbParameters.Append(list[j].DbParameters);
                    }

                    batchSql.Sql = string.Join(";\r\n", sqls);
                    BatchSqls.Add(batchSql);
                }
                sb.Append(string.Join(";\r\n", BatchSqls.Select(s => s.Sql)));
            }
            else
            {
                var setStr = string.Join(",", EntityDbMapping.EntityInfos.Where(w => !w.IsPrimaryKey).Select(s => $"{identifier.Insert(1, s.ColumnName)} = {parameterSymbol}{s.Identity}"));
                sb.Append($"UPDATE {identifier.Insert(1, TableName)} SET {setStr}");
                if (Where.Count > 0)
                {
                    sb.Append($" WHERE {string.Join(" AND ", Where)}");
                }
                else
                {
                    var entityDbMapping = EntityDbMapping.EntityInfos.FirstOrDefault(f => f.IsPrimaryKey);
                    if (entityDbMapping == null)
                    {
                        throw new Exception("无更新条件且未获取到KeyAuttribue特性标记属性,安全起见如需更新全表请使用Where方法,示例:Where(w=>true).");
                    }
                    else
                    {
                        sb.Append($" WHERE {entityDbMapping.ColumnName} = {parameterSymbol}{entityDbMapping.Identity}");
                    }
                }
            }
            var sql = sb.ToString();
            return sql;
        }
    }
}

