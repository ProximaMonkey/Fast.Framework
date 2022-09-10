using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Fast.Framework.Extensions
{

    /// <summary>
    /// DbDataReader扩展类
    /// </summary>
    public static class DbDataReaderExtensions
    {
        /// <summary>
        /// 获取方法缓存
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> getMethodCache;

        /// <summary>
        /// 转换方法名称
        /// </summary>
        private static readonly Dictionary<Type, string> convertMethodName;

        /// <summary>
        /// 表达式缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, Lazy<object>> lambdaCache;

        /// <summary>
        /// 是否DBNull方法
        /// </summary>
        private static readonly MethodInfo isDBNullMethod;

        #region 初始化
        /// <summary>
        /// 静态构造方法
        /// </summary>
        static DbDataReaderExtensions()
        {
            getMethodCache = new Dictionary<Type, MethodInfo>();
            convertMethodName = new Dictionary<Type, string>()
            {
                { typeof(short),"ToInt16"},
                { typeof(int),"ToInt32"},
                { typeof(long),"ToInt64"},
                { typeof(float),"ToSingle"},
                { typeof(double),"ToDouble"},
                { typeof(decimal),"ToDecimal"},
                { typeof(char),"ToChar"},
                { typeof(byte),"ToByte"},
                { typeof(bool),"ToBoolean"},
                { typeof(string),"ToString"},
                { typeof(DateTime),"ToDateTime"}
            };
            var names = new List<string>()
            {
                "GetInt16",
                "GetInt32",
                "GetInt64",
                "GetFloat",
                "GetDouble",
                "GetDecimal",
                "GetChar",
                "GetByte",
                "GetGuid",
                "GetBoolean",
                "GetString",
                "GetDateTime"
            };
            var methods = typeof(DbDataReader).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var getMethods = methods.Where(w => names.Exists(e => e == w.Name));
            foreach (var method in getMethods)
            {
                getMethodCache.Add(method.ReturnType, method);
            }
            isDBNullMethod = methods.First(f => f.Name == "IsDBNull");
            lambdaCache = new ConcurrentDictionary<string, Lazy<object>>();
        }
        #endregion

        /// <summary>
        /// 数据绑定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbColumns">数据库列</param>
        /// <returns></returns>
        private static Func<DbDataReader, T> DataBinding<T>(this ReadOnlyCollection<DbColumn> dbColumns)
        {
            var type = typeof(T);
            var cacheKey = $"{type.FullName}_{string.Join("|", dbColumns.Select(s => $"{s.ColumnName}{s.ColumnOrdinal.Value}"))}";
            return lambdaCache.GetOrAdd(cacheKey, key => new Lazy<object>(() =>
            {
                var parameterExpression = Expression.Parameter(typeof(DbDataReader), "r");
                if (type.IsClass && type != typeof(string))
                {
                    var propertyInfos = type.GetProperties();
                    var arguments = new List<Expression>();
                    var memberBindings = new List<MemberBinding>();
                    var isAnonymousType = type.FullName.StartsWith("<>f__AnonymousType");
                    if (!isAnonymousType)
                    {
                        propertyInfos = propertyInfos.Where(w => w.CanWrite).ToArray();
                    }
                    for (int i = 0; i < dbColumns.Count; i++)
                    {
                        var name = dbColumns[i].ColumnName;
                        PropertyInfo propertyInfo = null;
                        if (isAnonymousType)
                        {
                            propertyInfo = propertyInfos.FirstOrDefault(f => f.Name == name);
                        }
                        else
                        {
                            propertyInfo = propertyInfos.FirstOrDefault(f => f.Name == name || f.IsDefined(typeof(ColumnAttribute)) && f.GetCustomAttribute<ColumnAttribute>().Name == name);
                        }
                        if (propertyInfo != null)
                        {
                            if (!getMethodCache.ContainsKey(dbColumns[i].DataType))
                            {
                                throw new Exception($"该类型不支持绑定{dbColumns[i].DataType.FullName}.");
                            }
                            var mapperType = propertyInfo.PropertyType;
                            var isNullable = false;
                            if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                            {
                                mapperType = propertyInfo.PropertyType.GenericTypeArguments[0];
                                isNullable = true;
                            }
                            var constantExpression = Expression.Constant(i);
                            var isDBNullMethodCall = Expression.Call(parameterExpression, isDBNullMethod, constantExpression);
                            Expression getValueExpression = Expression.Call(parameterExpression, getMethodCache[dbColumns[i].DataType], constantExpression);
                            if (mapperType != dbColumns[i].DataType)
                            {
                                if (mapperType.Equals(typeof(Guid)))
                                {
                                    getValueExpression = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(string) }), getValueExpression);
                                }
                                else
                                {
                                    if (!convertMethodName.ContainsKey(mapperType))
                                    {
                                        throw new Exception($"该类型转换不受支持{mapperType.FullName}.");
                                    }
                                    getValueExpression = Expression.Call(typeof(Convert).GetMethod(convertMethodName[mapperType], new Type[] { dbColumns[i].DataType }), getValueExpression);
                                }
                            }
                            var conditionalExpression = Expression.Condition(isDBNullMethodCall, Expression.Default(propertyInfo.PropertyType), isNullable ? Expression.Convert(getValueExpression, propertyInfo.PropertyType) : getValueExpression);
                            if (isAnonymousType)
                            {
                                arguments.Add(conditionalExpression);
                            }
                            else
                            {
                                memberBindings.Add(Expression.Bind(propertyInfo, conditionalExpression));
                            }
                        }
                    }
                    Expression initExpression = isAnonymousType ? Expression.New(type.GetConstructors()[0], arguments) : Expression.MemberInit(Expression.New(type), memberBindings);
                    var lambdaExpression = Expression.Lambda<Func<DbDataReader, T>>(initExpression, parameterExpression);
                    return lambdaExpression.Compile();
                }
                else
                {
                    if (!getMethodCache.ContainsKey(dbColumns[0].DataType))
                    {
                        throw new Exception($"该类型不支持绑定{dbColumns[0].DataType.FullName}.");
                    }
                    var mapperType = type;
                    var isNullable = false;
                    if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    {
                        mapperType = type.GenericTypeArguments[0];
                        isNullable = true;
                    }
                    var constantExpression = Expression.Constant(0);
                    var isDBNullMethodCall = Expression.Call(parameterExpression, isDBNullMethod, constantExpression);
                    Expression getValueExpression = Expression.Call(parameterExpression, getMethodCache[dbColumns[0].DataType], constantExpression);
                    if (mapperType != dbColumns[0].DataType)
                    {
                        if (mapperType.Equals(typeof(Guid)))
                        {
                            getValueExpression = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(string) }), getValueExpression);
                        }
                        else
                        {
                            if (!convertMethodName.ContainsKey(mapperType))
                            {
                                throw new Exception($"该类型转换不受支持{mapperType.FullName}.");
                            }
                            getValueExpression = Expression.Call(typeof(Convert).GetMethod(convertMethodName[mapperType], new Type[] { dbColumns[0].DataType }), getValueExpression);
                        }
                    }
                    var conditionalExpression = Expression.Condition(isDBNullMethodCall, Expression.Default(type), isNullable ? Expression.Convert(getValueExpression, type) : getValueExpression);
                    var lambdaExpression = Expression.Lambda<Func<DbDataReader, T>>(conditionalExpression, parameterExpression);
                    return lambdaExpression.Compile();
                }
            })).Value as Func<DbDataReader, T>;
        }

        /// <summary>
        /// 最终处理
        /// </summary>
        /// <param name="reader">阅读器</param>
        /// <returns></returns>
        private static async Task FinalProcessing(this DbDataReader reader)
        {
            if (!await reader.NextResultAsync())
            {
                await reader.CloseAsync();
            };
        }

        /// <summary>
        /// 第一构建异步
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="dataReader">数据读取</param>
        /// <returns></returns>
        public static async Task<T> FristBuildAsync<T>(this Task<DbDataReader> dataReader)
        {
            var reader = await dataReader;
            var dbColumns = await reader.GetColumnSchemaAsync();
            var type = typeof(T);
            T t = default;
            if (await reader.ReadAsync())
            {
                var func = dbColumns.DataBinding<T>();
                t = func.Invoke(reader);
            }
            await reader.FinalProcessing();
            return t;
        }

        /// <summary>
        /// 列表构建异步
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="dataReader">数据读取</param>
        /// <returns></returns>
        public static async Task<List<T>> ListBuildAsync<T>(this Task<DbDataReader> dataReader)
        {
            var reader = await dataReader;
            var dbColumns = await reader.GetColumnSchemaAsync();
            var type = typeof(T);
            var data = new List<T>();
            var func = dbColumns.DataBinding<T>();
            while (await reader.ReadAsync())
            {
                data.Add(func.Invoke(reader));
            }
            await reader.FinalProcessing();
            return data;
        }

        /// <summary>
        /// 字典构建异步
        /// </summary>
        /// <param name="dataReader">数据读取</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, object>> DictionaryBuildAsync(this Task<DbDataReader> dataReader)
        {
            var reader = await dataReader;
            var data = new Dictionary<string, object>();
            var dbColumns = await reader.GetColumnSchemaAsync();
            if (dbColumns.Count > 0 && await reader.ReadAsync())
            {
                data = new Dictionary<string, object>();
                foreach (var c in dbColumns)
                {
                    data.Add(c.ColumnName, reader.IsDBNull(c.ColumnOrdinal.Value) ? null : reader.GetValue(c.ColumnOrdinal.Value));
                }
            }
            await reader.FinalProcessing();
            return data;
        }

        /// <summary>
        /// 字典列表构建异步
        /// </summary>
        /// <param name="dataReader">数据读取</param>
        /// <returns></returns>
        public static async Task<List<Dictionary<string, object>>> DictionaryListBuildAsync(this Task<DbDataReader> dataReader)
        {
            var reader = await dataReader;
            var data = new List<Dictionary<string, object>>();
            var dbColumns = await reader.GetColumnSchemaAsync();
            if (dbColumns.Count > 0)
            {
                while (await reader.ReadAsync())
                {
                    var keyValues = new Dictionary<string, object>();
                    foreach (var c in dbColumns)
                    {
                        keyValues.Add(c.ColumnName, reader.IsDBNull(c.ColumnOrdinal.Value) ? null : reader.GetValue(c.ColumnOrdinal.Value));
                    }
                    data.Add(keyValues);
                }
            }
            await reader.FinalProcessing();
            return data;
        }

    }
}
