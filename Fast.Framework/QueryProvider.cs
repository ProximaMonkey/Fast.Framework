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

    #region T1
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryProvider<T> : IQuery<T>
    {

        /// <summary>
        /// 查询构建
        /// </summary>
        public IQueryBuilder QueryBuilder { get; }

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 使用别名
        /// </summary>
        private bool useAlias;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder)
        {
            this.ado = ado;
            QueryBuilder = queryBuilder;
        }

        /// <summary>
        /// 去重
        /// </summary>
        /// <returns></returns>
        public IQuery<T> Distinct()
        {
            QueryBuilder.Distinct = true;
            return this;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2> Join<T2>(string joinType, Expression<Func<T, T2, bool>> expression)
        {
            var type = typeof(T2);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {ado.DbOptions.DbType.MappingIdentifier().Insert(1, type.GetTableName())} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// In
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields">字段</param>
        /// <param name="inType">类型</param>
        /// <param name="list">列表</param>
        /// <returns></returns>
        private IQuery<T> In<FieldsType>(string fields, string inType, List<FieldsType> list)
        {
            var dictionary = list.GenerateDbParameters();
            QueryBuilder.Where.Add($"{fields} {inType} ( {string.Join(",", dictionary.Keys.Select(s => $"{ado.DbOptions.DbType.MappingParameterSymbol()}{s}"))} )");
            return this;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2> LeftJoin<T2>(Expression<Func<T, T2, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2> RightJoin<T2>(Expression<Func<T, T2, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2> InnerJoin<T2>(Expression<Func<T, T2, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// In查询
        /// </summary>
        /// <typeparam name="FieldsType"></typeparam>
        /// <param name="fields">字段</param>
        /// <param name="list">列表</param>
        /// <returns></returns>
        public IQuery<T> In<FieldsType>(string fields, params FieldsType[] list)
        {
            return In(fields, list.ToList());
        }

        /// <summary>
        /// In查询
        /// </summary>
        /// <typeparam name="FieldsType"></typeparam>
        /// <param name="fields">字段</param>
        /// <param name="list">列表</param>
        /// <returns></returns>
        public IQuery<T> In<FieldsType>(string fields, List<FieldsType> list)
        {
            return In(fields, "IN", list);
        }

        /// <summary>
        /// NotIn查询
        /// </summary>
        /// <typeparam name="FieldsType"></typeparam>
        /// <param name="fields">字段</param>
        /// <param name="list">列表</param>
        /// <returns></returns>
        public IQuery<T> NotIn<FieldsType>(string fields, params FieldsType[] list)
        {
            return NotIn(fields, list.ToList());
        }

        /// <summary>
        /// NotIn查询
        /// </summary>
        /// <typeparam name="FieldsType"></typeparam>
        /// <param name="fields">字段</param>
        /// <param name="list">列表</param>
        /// <returns></returns>
        public IQuery<T> NotIn<FieldsType>(string fields, List<FieldsType> list)
        {
            return In(fields, "NOT IN", list);
        }

        /// <summary>
        /// 作为
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public IQuery<T> As(string tableName)
        {
            QueryBuilder.TableName = tableName;
            return this;
        }

        /// <summary>
        /// 使用别名
        /// </summary>
        /// <returns></returns>
        public IQuery<T> UseAlias()
        {
            useAlias = true;
            return this;
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T> Where(Expression<Func<T, bool>> expression)
        {
            if (useAlias)
            {
                QueryBuilder.Alias = expression.Parameters[0].Name;
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where,
                IgnoreParameter = QueryBuilder.Join.Count == 0 && !useAlias
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <typeparam name="Table"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T> Where<Table>(Expression<Func<T, Table, bool>> expression)
        {
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T> GroupBy(Expression<Func<T, object>> expression)
        {
            if (useAlias)
            {
                QueryBuilder.Alias = expression.Parameters[0].Name;
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy,
                IgnoreParameter = QueryBuilder.Join.Count == 0 && !useAlias
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T> Having(Expression<Func<T, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            if (useAlias)
            {
                QueryBuilder.Alias = expression.Parameters[0].Name;
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where,
                IgnoreParameter = QueryBuilder.Join.Count == 0 && !useAlias
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="orderFields">排序字段</param>
        /// <param name="orderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T> OrderBy(string orderFields, string orderType = "ASC")
        {
            QueryBuilder.OrderBy.Add($"{orderFields} {orderType}");
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T> OrderBy(Expression<Func<T, object>> expression, string oderType = "ASC")
        {
            if (useAlias)
            {
                QueryBuilder.Alias = expression.Parameters[0].Name;
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy,
                IgnoreParameter = QueryBuilder.Join.Count == 0 && !useAlias
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> expression)
        {
            if (useAlias)
            {
                QueryBuilder.Alias = expression.Parameters[0].Name;
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select,
                IgnoreParameter = QueryBuilder.Join.Count == 0 && !useAlias
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

        /// <summary>
        /// 第一异步
        /// </summary>
        /// <returns></returns>
        public Task<T> FristAsync()
        {
            var sql = QueryBuilder.ToSql();
            return ado.ExecuteReaderAsync(CommandType.Text, sql, ado.CreateParameter(QueryBuilder.DbParameters)).FristBuildAsync<T>();
        }

        /// <summary>
        /// 到列表异步
        /// </summary>
        /// <returns></returns>
        public Task<List<T>> ToListAsync()
        {
            var sql = QueryBuilder.ToSql();
            return ado.ExecuteReaderAsync(CommandType.Text, sql, ado.CreateParameter(QueryBuilder.DbParameters)).ListBuildAsync<T>();
        }

        /// <summary>
        /// 到页列表异步
        /// </summary>
        /// <param name="pagination">页对象</param>
        /// <returns></returns>
        public async Task<PageData<List<T>>> ToPageListAsync(Pagination pagination)
        {
            var pageSql = "";
            if (ado.DbOptions.DbType == Models.DbType.SQLServer)
            {
                pageSql = string.Format("SELECT * FROM ( SELECT ROW_NUMBER() OVER (ORDER BY (SELECT 0)) row_id,x.* FROM (\r\n{0}\r\n) x ) x WHERE x.row_id BETWEEN {1} AND {2}", QueryBuilder.ToSql(), (pagination.Page - 1) * pagination.PageSize + 1, pagination.Page * pagination.PageSize);
            }
            else if (ado.DbOptions.DbType == Models.DbType.Oracle)
            {
                pageSql = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY 0) \"row_id\",\"x\".* FROM ({0}) \"x\") \"x\" WHERE \"x\".\"row_id\" BETWEEN {1} AND {2}", QueryBuilder.ToSql(), (pagination.Page - 1) * pagination.PageSize + 1, pagination.Page * pagination.PageSize);
            }
            else if (ado.DbOptions.DbType == Models.DbType.MySQL || ado.DbOptions.DbType == Models.DbType.PostgreSQL || ado.DbOptions.DbType == Models.DbType.SQLite)
            {
                pageSql = string.Format("SELECT * FROM ( \r\n{0}\r\n ) x LIMIT {1} OFFSET {2}", QueryBuilder.ToSql(), pagination.PageSize, (pagination.Page - 1) * pagination.PageSize);
            }
            else
            {
                throw new NotSupportedException($"{ado.DbOptions.DbType} 不支持的数据库类型");
            }
            var data = await ado.ExecuteReaderAsync(CommandType.Text, pageSql, ado.CreateParameter(QueryBuilder.DbParameters)).ListBuildAsync<T>();
            var count = await CountAsync();
            return new PageData<List<T>>() { Data = data, Count = count };
        }

        /// <summary>
        /// 到字典异步
        /// </summary>
        /// <returns></returns>
        public Task<Dictionary<string, object>> ToDictionaryAsync()
        {
            var sql = QueryBuilder.ToSql();
            return ado.ExecuteReaderAsync(CommandType.Text, sql, ado.CreateParameter(QueryBuilder.DbParameters)).DictionaryBuildAsync();
        }

        /// <summary>
        /// 到字典列表异步
        /// </summary>
        /// <returns></returns>
        public Task<List<Dictionary<string, object>>> ToDictionaryListAsync()
        {
            var sql = QueryBuilder.ToSql();
            return ado.ExecuteReaderAsync(CommandType.Text, sql, ado.CreateParameter(QueryBuilder.DbParameters)).DictionaryListBuildAsync();
        }

        /// <summary>
        /// 计数异步
        /// </summary>
        /// <returns></returns>
        public Task<int> CountAsync()
        {
            var sql = QueryBuilder.ToSql();
            var countTemplate = "SELECT COUNT(1) AS Qty FROM ({0}) x";
            return ado.ExecuteScalarAsync<int>(CommandType.Text, string.Format(countTemplate, sql), ado.CreateParameter(QueryBuilder.DbParameters));
        }

        /// <summary>
        /// 任何异步
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AnyAsync()
        {
            return await CountAsync() > 0;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="InsertTable">实体</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public Task<int> Insert<InsertTable>(Expression<Func<InsertTable, object>> expression)
        {
            var type = typeof(InsertTable);
            QueryBuilder.InsertTableName = type.GetTableName();
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                IgnoreParameter = true,
                ResolveSqlType = ResolveSqlType.Column
            });
            QueryBuilder.InsertColumns = result.SqlString;
            return ado.ExecuteNonQueryAsync(CommandType.Text, QueryBuilder.ToSql(), ado.CreateParameter(QueryBuilder.DbParameters));
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columns">列名称</param>
        /// <returns></returns>
        public Task<int> Insert(string tableName, params string[] columns)
        {
            return Insert(tableName, columns.ToList());
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columns">列名称</param>
        /// <returns></returns>
        public Task<int> Insert(string tableName, List<string> columns)
        {
            QueryBuilder.InsertTableName = tableName;
            QueryBuilder.InsertColumns = string.Join(",", columns);
            return ado.ExecuteNonQueryAsync(CommandType.Text, QueryBuilder.ToSql(), ado.CreateParameter(QueryBuilder.DbParameters));
        }
    }
    #endregion

    #region T2
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class QueryProvider<T, T2> : QueryProvider<T>, IQuery<T, T2>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3> Join<T3>(string joinType, Expression<Func<T, T2, T3, bool>> expression)
        {
            var type = typeof(T3);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3> LeftJoin<T3>(Expression<Func<T, T2, T3, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3> RightJoin<T3>(Expression<Func<T, T2, T3, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3> InnerJoin<T3>(Expression<Func<T, T2, T3, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2> Where(Expression<Func<T, T2, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2> GroupBy(Expression<Func<T, T2, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2> Having(Expression<Func<T, T2, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2> OrderBy(Expression<Func<T, T2, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T3
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class QueryProvider<T, T2, T3> : QueryProvider<T, T2>, IQuery<T, T2, T3>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T4"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4> Join<T4>(string joinType, Expression<Func<T, T2, T3, T4, bool>> expression)
        {
            var type = typeof(T4);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T4"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4> LeftJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T4"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4> RightJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T4"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4> InnerJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3> Where(Expression<Func<T, T2, T3, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3> GroupBy(Expression<Func<T, T2, T3, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3> Having(Expression<Func<T, T2, T3, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3> OrderBy(Expression<Func<T, T2, T3, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T4
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    public class QueryProvider<T, T2, T3, T4> : QueryProvider<T, T2, T3>, IQuery<T, T2, T3, T4>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T5"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5> Join<T5>(string joinType, Expression<Func<T, T2, T3, T4, T5, bool>> expression)
        {
            var type = typeof(T5);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T5"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T5"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5> RightJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T5"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4> GroupBy(Expression<Func<T, T2, T3, T4, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4> Having(Expression<Func<T, T2, T3, T4, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4> OrderBy(Expression<Func<T, T2, T3, T4, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T5
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5> : QueryProvider<T, T2, T3, T4>, IQuery<T, T2, T3, T4, T5>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T6"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5, T6> Join<T6>(string joinType, Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
        {
            var type = typeof(T6);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5, T6>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T6"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6> LeftJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T6"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6> RightJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T6"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6> InnerJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5> Where(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5> GroupBy(Expression<Func<T, T2, T3, T4, T5, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5> Having(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5> OrderBy(Expression<Func<T, T2, T3, T4, T5, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T6
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5, T6> : QueryProvider<T, T2, T3, T4, T5>, IQuery<T, T2, T3, T4, T5, T6>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T7"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5, T6, T7> Join<T7>(string joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            var type = typeof(T7);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5, T6, T7>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T7"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7> LeftJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T7"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7> RightJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T7"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7> InnerJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6> Where(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6> GroupBy(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6> Having(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6> OrderBy(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T7
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5, T6, T7> : QueryProvider<T, T2, T3, T4, T5, T6>, IQuery<T, T2, T3, T4, T5, T6, T7>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T8"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5, T6, T7, T8> Join<T8>(string joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            var type = typeof(T8);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5, T6, T7, T8>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T8"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T8"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T8"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7> GroupBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7> OrderBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T8
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5, T6, T7, T8> : QueryProvider<T, T2, T3, T4, T5, T6, T7>, IQuery<T, T2, T3, T4, T5, T6, T7, T8>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T9"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> Join<T9>(string joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            var type = typeof(T9);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T9"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T9"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T9"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8> GroupBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8> OrderBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T9
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9> : QueryProvider<T, T2, T3, T4, T5, T6, T7, T8>, IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T10"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Join<T10>(string joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            var type = typeof(T10);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T10"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T10"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T10"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T10
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> : QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9>, IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T11"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Join<T11>(string joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            var type = typeof(T11);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T11"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T11"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T11"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T11
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T12"></typeparam>
        /// <param name="joinType">连接类型</param>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Join<T12>(string joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            var type = typeof(T12);
            QueryBuilder.Alias = expression.Parameters[0].Name;
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Join.Add($"{joinType} JOIN {type.GetTableName()} {expression.Parameters[1].Name} ON {result.SqlString}");
            QueryBuilder.DbParameters.Append(result.DbParameters);
            var queryProvider = new QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ado, QueryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 左连接
        /// </summary>
        /// <typeparam name="T12"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return Join("LEFT", expression);
        }

        /// <summary>
        /// 右连接
        /// </summary>
        /// <typeparam name="T12"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return Join("RIGHT", expression);
        }

        /// <summary>
        /// 内连接
        /// </summary>
        /// <typeparam name="T12"></typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            return Join("INNER", expression);
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion

    #region T12
    /// <summary>
    /// 查询提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="T10"></typeparam>
    /// <typeparam name="T11"></typeparam>
    /// <typeparam name="T12"></typeparam>
    public class QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : QueryProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {

        /// <summary>
        /// Ado
        /// </summary>
        private readonly IAdo ado;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="ado">Ado</param>
        /// <param name="queryBuilder">查询构造</param>
        public QueryProvider(IAdo ado, IQueryBuilder queryBuilder) : base(ado, queryBuilder)
        {
            this.ado = ado;
        }

        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Where.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.GroupBy
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.GroupBy.Add(result.SqlString);
            return this;
        }

        /// <summary>
        /// 有
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression)
        {
            if (QueryBuilder.GroupBy.Count == 0)
            {
                throw new Exception("必须包含GroupBy方法才可以使用Having方法");
            }
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Where
            });
            QueryBuilder.Having.Add(result.SqlString);
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="oderType">排序类型</param>
        /// <returns></returns>
        public IQuery<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>> expression, string oderType = "ASC")
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.OrderBy
            });
            QueryBuilder.OrderBy.Add($"{result.SqlString} {oderType}");
            return this;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> expression)
        {
            var result = expression.ResolveSql(new ResolveSqlOptions()
            {
                DbType = ado.DbOptions.DbType,
                ResolveSqlType = ResolveSqlType.Select
            });
            QueryBuilder.Columns = result.SqlString;
            QueryBuilder.DbParameters.Append(result.DbParameters);
            return new QueryProvider<TResult>(ado, QueryBuilder);
        }

    }
    #endregion
}

