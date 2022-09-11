using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Transactions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Fast.Framework.Interfaces;
using Fast.Framework.Extensions;
using Fast.Framework.Aop;
using Fast.Framework.Models;


namespace Fast.Framework
{

    /// <summary>
    /// 数据库上下文实现类
    /// </summary>
    public class DbContext : IDbContext
    {
        /// <summary>
        /// Ado
        /// </summary>
        public IAdo Ado { get; private set; }

        /// <summary>
        /// Aop
        /// </summary>
        public IAop Aop { get; }

        /// <summary>
        /// 选项
        /// </summary>
        private readonly List<DbOptions> options;

        /// <summary>
        /// ado缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, IAdo> adoCache;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="options">选项</param>
        public DbContext(IOptionsSnapshot<List<DbOptions>> options) : this(options.Value)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="options">选项</param>
        public DbContext(List<DbOptions> options)
        {
            if (options == null || options.Count == 0)
            {
                throw new ArgumentException($"{nameof(options)}不包含任何元素.");
            }

            var list = options.GroupBy(g => g.DbId).Where(a => a.Count() > 1);

            if (list.Any())
            {
                throw new Exception($"数据库ID {string.Join(",", list.Select(s => s.Key))} 重复.");
            }

            this.options = options;
            adoCache = new ConcurrentDictionary<string, IAdo>();

            Aop = new AopProvider();
            Aop.Change += Aop_Change;

            var option = options.FirstOrDefault(f => f.IsDefault);
            if (option == null)
            {
                option = options[0];
            }
            ChangeDb(option.DbId);
        }

        /// <summary>
        /// Aop改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Aop_Change(object sender, EventArgs e)
        {
            if (Ado is not DispatchProxy)
            {
                Ado = DynamicProxyFactory.Create<IAdo>(Ado, new AdoIntercept(Aop));
            }
        }

        #region 多租户接口实现
        /// <summary>
        /// 改变数据库
        /// </summary>
        /// <param name="dbId">数据库ID</param>
        /// <returns></returns>
        public void ChangeDb(string dbId)
        {
            Ado = adoCache.GetOrAdd(dbId, k =>
            {
                var option = options.FirstOrDefault(f => f.DbId == dbId);
                if (option == null)
                {
                    throw new Exception($"更改数据库失败,ID {dbId} 不存在.");
                }
                var adoProvider = new AdoProvider(option);
                adoProvider.TestConnection();// 热连接创建连接池
                return adoProvider;
            });
            Aop.DbLog = Aop.DbLog;// 触发代理对象创建
        }
        #endregion

        #region 增 删 改
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public IInsert<T> Insert<T>(T entity) where T : class
        {
            var type = typeof(T);
            var insertBuilder = new InsertBuilder(Ado.DbOptions.DbType);
            insertBuilder.TableName = type.GetTableName();
            var keyAuttribute = typeof(KeyAttribute);
            var entityDbMapping = entity.GetEntityDbMapping(0, p =>
            {
                var isDefined = p.IsDefined(keyAuttribute);
                return !isDefined || isDefined && p.PropertyType.Equals(typeof(string));
            });
            insertBuilder.EntityDbMapping = entityDbMapping;
            var insertProvider = new InsertProvider<T>(Ado, insertBuilder);
            return insertProvider;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entitys">实体</param>
        /// <returns></returns>
        public IInsert<T> Insert<T>(List<T> entitys) where T : class
        {
            var type = typeof(T);
            var insertBuilder = new InsertBuilder(Ado.DbOptions.DbType);
            insertBuilder.TableName = type.GetTableName();
            var keyAuttribute = typeof(KeyAttribute);
            var entityDbMappings = entitys.GetEntityDbMappings(p =>
            {
                var isDefined = p.IsDefined(keyAuttribute);
                return !isDefined || isDefined && p.PropertyType.Equals(typeof(string));
            });
            insertBuilder.EntityDbMappings = entityDbMappings;
            insertBuilder.IsBatch = true;
            var insertProvider = new InsertProvider<T>(Ado, insertBuilder);
            return insertProvider;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="dictionary">字典</param>
        /// <returns></returns>
        public IInsert<T> Insert<T>(Dictionary<string, object> dictionary) where T : class
        {
            var type = typeof(T);
            var insertBuilder = new InsertBuilder(Ado.DbOptions.DbType);
            insertBuilder.TableName = type.GetTableName();
            insertBuilder.EntityDbMapping = new EntityDbMapping()
            {
                EntityInfos = dictionary.Select(s => new EntityInfo()
                {
                    Identity = s.Key,
                    PropertyValue = s.Value,
                    ColumnName = s.Key
                }).ToList(),
                DbParameters = dictionary
            };
            var insertProvider = new InsertProvider<T>(Ado, insertBuilder);
            return insertProvider;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="dictionarys">字典</param>
        /// <returns></returns>
        public IInsert<T> Insert<T>(List<Dictionary<string, object>> dictionarys) where T : class
        {
            var type = typeof(T);
            var insertBuilder = new InsertBuilder(Ado.DbOptions.DbType);
            insertBuilder.TableName = type.GetTableName();
            var parameterIndex = 0;
            foreach (var item in dictionarys)
            {
                parameterIndex++;
                insertBuilder.EntityDbMappings.Add(new EntityDbMapping()
                {
                    EntityInfos = item.Select(s => new EntityInfo()
                    {
                        Identity = $"{s.Key}_{parameterIndex}",
                        PropertyValue = s.Value,
                        ColumnName = s.Key
                    }).ToList(),
                    DbParameters = item.Identity(parameterIndex)
                });
            }
            insertBuilder.IsBatch = true;
            var insertProvider = new InsertProvider<T>(Ado, insertBuilder);
            return insertProvider;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDelete<T> Delete<T>() where T : class
        {
            var type = typeof(T);
            var deleteBuilder = new DeleteBuilder(Ado.DbOptions.DbType);
            deleteBuilder.TableName = type.GetTableName();
            var deleteProvider = new DeleteProvider<T>(Ado, deleteBuilder);
            return deleteProvider;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public IDelete<T> Delete<T>(T entity) where T : class
        {
            var type = typeof(T);
            var deleteBuilder = new DeleteBuilder(Ado.DbOptions.DbType);
            deleteBuilder.TableName = type.GetTableName();
            var primaryKeys = entity.GetPrimaryKeyValues();
            if (primaryKeys.Count == 0)
            {
                throw new ArgumentNullException(nameof(entity), "未获取到标记KeyAuttribute特性属性.");
            }
            var primaryKey = primaryKeys.First();
            var parameterName = $"{primaryKey.Key}_1";
            deleteBuilder.Where.Add($" {Ado.DbOptions.DbType.MappingIdentifier().Insert(1, primaryKey.Key)} = {Ado.DbOptions.DbType.MappingParameterSymbol()}{parameterName}");
            deleteBuilder.DbParameters.Add(parameterName, primaryKey.Value);
            var deleteProvider = new DeleteProvider<T>(Ado, deleteBuilder);
            return deleteProvider;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public IUpdate<T> Update<T>(T entity) where T : class
        {
            var type = typeof(T);
            var updateBuilder = new UpdateBuilder(Ado.DbOptions.DbType);
            updateBuilder.TableName = type.GetTableName();
            var entityDbMapping = entity.GetEntityDbMapping();
            updateBuilder.EntityDbMapping = entityDbMapping;
            var updateProvider = new UpdateProvider<T>(Ado, updateBuilder);
            return updateProvider;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys">实体</param>
        /// <returns></returns>
        public IUpdate<T> Update<T>(List<T> entitys) where T : class
        {
            var type = typeof(T);
            var updateBuilder = new UpdateBuilder(Ado.DbOptions.DbType);
            updateBuilder.TableName = type.GetTableName();
            var entityDbMappings = entitys.GetEntityDbMappings();
            updateBuilder.EntityDbMappings = entityDbMappings;
            updateBuilder.IsBatch = true;
            var updateProvider = new UpdateProvider<T>(Ado, updateBuilder);
            return updateProvider;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary">字典</param>
        /// <returns></returns>
        public IUpdate<T> Update<T>(Dictionary<string, object> dictionary) where T : class
        {
            var type = typeof(T);
            var updateBuilder = new UpdateBuilder(Ado.DbOptions.DbType);
            updateBuilder.TableName = type.GetTableName();
            updateBuilder.EntityDbMapping = new EntityDbMapping()
            {
                EntityInfos = dictionary.Select(s => new EntityInfo()
                {
                    Identity = s.Key,
                    PropertyValue = s.Value,
                    ColumnName = s.Key
                }).ToList(),
                DbParameters = dictionary
            };
            var updateProvider = new UpdateProvider<T>(Ado, updateBuilder);
            return updateProvider;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionarys">字典</param>
        /// <returns></returns>
        public IUpdate<T> Update<T>(List<Dictionary<string, object>> dictionarys) where T : class
        {
            var type = typeof(T);
            var updateBuilder = new UpdateBuilder(Ado.DbOptions.DbType);
            updateBuilder.TableName = type.GetTableName();
            var parameterIndex = 0;
            foreach (var item in dictionarys)
            {
                parameterIndex++;
                updateBuilder.EntityDbMappings.Add(new EntityDbMapping()
                {
                    EntityInfos = item.Select(s => new EntityInfo()
                    {
                        Identity = $"{s.Key}_{parameterIndex}",
                        PropertyValue = s.Value,
                        ColumnName = s.Key
                    }).ToList(),
                    DbParameters = item.Identity(parameterIndex)
                });
            }
            updateBuilder.IsBatch = true;
            var updateProvider = new UpdateProvider<T>(Ado, updateBuilder);
            return updateProvider;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 子查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query">查询对象</param>
        /// <returns></returns>
        public TResult SubQuery<TResult>(IQuery query)
        {
            return default;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQuery<T> Query<T>() where T : class
        {
            var type = typeof(T);
            var queryBuilder = new QueryBuilder(Ado.DbOptions.DbType);
            queryBuilder.TableName = type.GetTableName();
            return new QueryProvider<T>(Ado, queryBuilder);
        }

        /// <summary>
        /// 联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isAll">是否全联合</param>
        /// <param name="querys">查询集合</param>
        /// <returns></returns>
        private IQuery<T> Union<T>(bool isAll, List<IQuery<T>> querys)
        {
            if (querys.Count < 2)
            {
                throw new Exception($"至少有两个查询对象,否则没有必要使用{(isAll ? "Union All" : "Union")}查询.");
            }
            var queryBuilder = new QueryBuilder(Ado.DbOptions.DbType);
            var sqlList = new List<string>();
            foreach (var item in querys)
            {
                sqlList.Add(item.QueryBuilder.ToSql());
                queryBuilder.DbParameters.Append(item.QueryBuilder.DbParameters);
            }
            queryBuilder.Union = string.Join(isAll ? "\r\nUNION ALL\r\n" : "\r\nUNION\r\n", sqlList);
            queryBuilder.TableName = $"{(isAll ? "UnionALL" : "Union")}_{querys.Count}";
            var queryProvider = new QueryProvider<T>(Ado, queryBuilder);
            return queryProvider;
        }

        /// <summary>
        /// 联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象数组</param>
        /// <returns></returns>
        public IQuery<T> Union<T>(params IQuery<T>[] querys)
        {
            return Union(querys.ToList());
        }

        /// <summary>
        /// 联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象列表</param>
        /// <returns></returns>
        public IQuery<T> Union<T>(List<IQuery<T>> querys)
        {
            return Union<T>(false, querys);
        }

        /// <summary>
        /// 全联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象数组</param>
        /// <returns></returns>
        public IQuery<T> UnionAll<T>(params IQuery<T>[] querys)
        {
            return UnionAll(querys.ToList());
        }

        /// <summary>
        /// 全联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象列表</param>
        /// <returns></returns>
        public IQuery<T> UnionAll<T>(List<IQuery<T>> querys)
        {
            return Union<T>(true, querys);
        }
        #endregion
    }
}
