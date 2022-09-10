using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fast.Framework.Interfaces;
using Fast.Framework.Models;

namespace Fast.Framework
{

    /// <summary>
    /// 插入实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InsertProvider<T> : IInsert<T>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 插入构建
        /// </summary>
        public IInsertBuilder InsertBuilder { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="insertBuilder">插入构建</param>
        public InsertProvider(IAdo ado, IInsertBuilder insertBuilder)
        {
            this.ado = ado;
            this.InsertBuilder = insertBuilder;
        }

        /// <summary>
        /// 作为
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public IInsert<T> As(string tableName)
        {
            InsertBuilder.TableName = tableName;
            return this;
        }

        /// <summary>
        /// 执行异步
        /// </summary>
        /// <returns></returns>
        public async Task<int> ExceuteAsync()
        {
            var sql = InsertBuilder.ToSql();
            if (InsertBuilder.IsBatch)
            {
                var result = 0;
                await ado.BeginTranAsync();
                foreach (var item in InsertBuilder.BatchSqls)
                {
                    result += await ado.ExecuteNonQueryAsync(CommandType.Text, item.Sql, ado.CreateParameter(item.DbParameters));
                }
                await ado.CommitTranAsync();
                return result;
            }
            else
            {
                return await ado.ExecuteNonQueryAsync(CommandType.Text, sql, ado.CreateParameter(InsertBuilder.EntityDbMapping.DbParameters));
            }
        }

        /// <summary>
        /// 执行返回自增ID异步
        /// </summary>
        /// <returns></returns>
        public Task<int> ExceuteReturnIdentityAsync()
        {
            var sql = InsertBuilder.ToSql();
            if (ado.DbOptions.DbType == Models.DbType.SQLServer || ado.DbOptions.DbType == Models.DbType.MySQL)
            {
                sql = $"{(sql.LastIndexOf(';') == -1 ? $"{sql};" : sql)}SELECT @@IDENTITY;";
            }
            else if (ado.DbOptions.DbType == Models.DbType.SQLite)
            {
                sql = $"{(sql.LastIndexOf(';') == -1 ? $"{sql};" : sql)}SELECT LAST_INSERT_ROWID();";
            }
            else
            {
                throw new NotSupportedException($"不支持返回自增ID");
            }
            return ado.ExecuteScalarAsync<int>(CommandType.Text, sql, ado.CreateParameter(InsertBuilder.EntityDbMapping.DbParameters));
        }
    }
}
