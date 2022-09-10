using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fast.Framework.Interfaces;
using Fast.Framework.Extensions;
using Fast.Framework.Models;


namespace Fast.Framework
{

    /// <summary>
    /// 删除实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeleteProvider<T> : IDelete<T> where T : class
    {

        /// <summary>
        /// 删除构建
        /// </summary>
        public IDeleteBuilder DeleteBuilder { get; }

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">ado</param>
        /// <param name="deleteBuilder">删除构建</param>
        public DeleteProvider(IAdo ado, IDeleteBuilder deleteBuilder)
        {
            this.ado = ado;
            this.DeleteBuilder = deleteBuilder;
        }

        /// <summary>
        /// 作为
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public IDelete<T> As(string tableName)
        {
            DeleteBuilder.TableName = tableName;
            return this;
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="columnName">列名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public IDelete<T> WhereColumn(string columnName, object value)
        {
            if (DeleteBuilder.DbParameters.ContainsKey(columnName))
            {
                throw new Exception($"列名称{columnName}已存在条件,不允许重复添加.");
            }
            var whereStr = $"{ado.DbOptions.DbType.MappingIdentifier().Insert(1, columnName)} = {ado.DbOptions.DbType.MappingParameterSymbol()}{columnName}";
            DeleteBuilder.Where.Add(whereStr);
            DeleteBuilder.DbParameters.Add(columnName, value);
            return this;
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="whereColumns">条件列</param>
        /// <returns></returns>
        public IDelete<T> WhereColumns(Dictionary<string, object> whereColumns)
        {
            var key = whereColumns.Keys.FirstOrDefault(f => DeleteBuilder.DbParameters.Keys.ToList().Exists(e => e == f));
            if (key != null)
            {
                throw new Exception($"列名称{key}已存在条件,不允许重复添加.");
            }
            var whereStr = whereColumns.Keys.Select(s => $"{ado.DbOptions.DbType.MappingIdentifier().Insert(1, s)} = {ado.DbOptions.DbType.MappingParameterSymbol()}{s}");
            DeleteBuilder.Where.Add(string.Join(" AND ", whereStr));
            DeleteBuilder.DbParameters.Append(whereColumns);
            return this;
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IDelete<T> Where(Expression<Func<T, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                IgnoreParameter = true,
                ResolveSqlType = ResolveSqlType.Where
            });
            DeleteBuilder.Where.Add(result.SqlString);
            DeleteBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public Task<int> ExceuteAsync()
        {
            return ado.ExecuteNonQueryAsync(CommandType.Text, DeleteBuilder.ToSql(), ado.CreateParameter(DeleteBuilder.DbParameters));
        }
    }
}

