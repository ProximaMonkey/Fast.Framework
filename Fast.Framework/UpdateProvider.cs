using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fast.Framework.Interfaces;
using Fast.Framework.Extensions;
using Fast.Framework.Models;

namespace Fast.Framework
{

    /// <summary>
    /// 更新实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UpdateProvider<T> : IUpdate<T> where T : class
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 更新建造者
        /// </summary>
        public IUpdateBuilder UpdateBuilder { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">ado</param>
        /// <param name="updateBuilder">更新建造者</param>
        public UpdateProvider(IAdo ado, IUpdateBuilder updateBuilder)
        {
            this.ado = ado;
            this.UpdateBuilder = updateBuilder;
        }

        /// <summary>
        /// 作为
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public IUpdate<T> As(string tableName)
        {
            UpdateBuilder.TableName = tableName;
            return this;
        }


        /// <summary>
        /// 列
        /// </summary>
        /// <param name="columns">列</param>
        /// <returns></returns>
        public IUpdate<T> Columns(params string[] columns)
        {
            return Columns(columns.ToList());
        }

        /// <summary>
        /// 列
        /// </summary>
        /// <param name="columns">列</param>
        /// <returns></returns>
        public IUpdate<T> Columns(List<string> columns)
        {
            if (UpdateBuilder.IsBatch)
            {
                foreach (var item in UpdateBuilder.EntityDbMappings)
                {
                    var list = item.EntityInfos.Where(r => !columns.Exists(e => e == r.ColumnName));

                    foreach (var entity in list)
                    {
                        item.DbParameters.Remove(entity.Identity);
                    }

                    item.EntityInfos.RemoveAll(r => !columns.Exists(e => e == r.ColumnName));
                }
            }
            else
            {
                var list = UpdateBuilder.EntityDbMapping.EntityInfos.Where(r => !columns.Exists(e => e == r.ColumnName));

                foreach (var entity in list)
                {
                    UpdateBuilder.EntityDbMapping.DbParameters.Remove(entity.Identity);
                }

                UpdateBuilder.EntityDbMapping.EntityInfos.RemoveAll(r => !columns.Exists(e => e == r.ColumnName));
            }
            return this;
        }

        /// <summary>
        /// 列
        /// </summary>
        /// <param name="expression">列</param>
        /// <returns></returns>
        public IUpdate<T> Columns(Expression<Func<T, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                IgnoreParameter = true,
                IgnoreIdentifier = true,
                ResolveSqlType = ResolveSqlType.Column
            });
            var list = result.SqlString.Split(",");
            return Columns(list);
        }

        /// <summary>
        /// 忽略列
        /// </summary>
        /// <param name="columns">列</param>
        /// <returns></returns>
        public IUpdate<T> IgnoreColumns(params string[] columns)
        {
            return IgnoreColumns(columns.ToList());
        }

        /// <summary>
        /// 忽略列
        /// </summary>
        /// <param name="columns">列</param>
        /// <returns></returns>
        public IUpdate<T> IgnoreColumns(List<string> columns)
        {
            if (UpdateBuilder.IsBatch)
            {
                foreach (var item in UpdateBuilder.EntityDbMappings)
                {
                    var list = item.EntityInfos.Where(w => columns.Exists(e => e == w.ColumnName));
                    foreach (var entity in list)
                    {
                        item.DbParameters.Remove(entity.Identity);
                    }
                    item.EntityInfos.RemoveAll(r => columns.Exists(e => e == r.ColumnName));
                }
            }
            else
            {
                var list = UpdateBuilder.EntityDbMapping.EntityInfos.Where(w => columns.Exists(e => e == w.ColumnName));
                foreach (var entity in list)
                {
                    UpdateBuilder.EntityDbMapping.DbParameters.Remove(entity.Identity);
                }
                UpdateBuilder.EntityDbMapping.EntityInfos.RemoveAll(r => columns.Exists(e => e == r.ColumnName));
            }
            return this;
        }

        /// <summary>
        /// 忽略列
        /// </summary>
        /// <param name="expression">列</param>
        /// <returns></returns>
        public IUpdate<T> IgnoreColumns(Expression<Func<T, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                IgnoreParameter = true,
                IgnoreIdentifier = true,
                ResolveSqlType = ResolveSqlType.Column
            });
            var list = result.SqlString.Split(",");
            return IgnoreColumns(list);
        }

        /// <summary>
        /// 条件列
        /// </summary>
        /// <param name="columns">列</param>
        /// <returns></returns>
        public IUpdate<T> WhereColumns(params string[] columns)
        {
            return WhereColumns(columns.ToList());
        }

        /// <summary>
        /// 条件列
        /// </summary>
        /// <param name="columns">列</param>
        /// <returns></returns>
        public IUpdate<T> WhereColumns(List<string> columns)
        {
            if (UpdateBuilder.IsBatch)
            {
                UpdateBuilder.EntityDbMappings = UpdateBuilder.EntityDbMappings.Select(l =>
                {
                    l.EntityInfos = l.EntityInfos.Select(s =>
                    {
                        s.IsPrimaryKey = columns.Exists(e => e == s.ColumnName); return s;
                    }).ToList();
                    return l;
                }).ToList();
            }
            else
            {
                UpdateBuilder.EntityDbMapping.EntityInfos = UpdateBuilder.EntityDbMapping.EntityInfos.Select(s => { s.IsPrimaryKey = columns.Exists(e => e == s.ColumnName); return s; }).ToList();
            }
            return this;
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IUpdate<T> Where(Expression<Func<T, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                IgnoreParameter = true,
                ResolveSqlType = ResolveSqlType.Where
            });
            UpdateBuilder.Where.Add(result.SqlString);
            UpdateBuilder.EntityDbMapping.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public async Task<int> ExceuteAsync()
        {
            var sql = UpdateBuilder.ToSql();
            if (UpdateBuilder.IsBatch)
            {
                var result = 0;
                await ado.BeginTranAsync();
                foreach (var item in UpdateBuilder.BatchSqls)
                {
                    result += await ado.ExecuteNonQueryAsync(CommandType.Text, item.Sql, ado.CreateParameter(item.DbParameters));
                }
                await ado.CommitTranAsync();
                return result;
            }
            else
            {
                return await ado.ExecuteNonQueryAsync(CommandType.Text, sql, ado.CreateParameter(UpdateBuilder.EntityDbMapping.DbParameters));
            }
        }
    }
}

