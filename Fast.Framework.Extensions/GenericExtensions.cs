using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fast.Framework.Models;


namespace Fast.Framework.Extensions
{

    /// <summary>
    /// 泛型扩展类
    /// </summary>
    public static class GenericExtensions
    {

        /// <summary>
        /// 实体信息缓存
        /// </summary>
        private readonly static ConcurrentDictionary<string, Lazy<List<EntityInfo>>> entityInfoCache;

        /// <summary>
        /// 静态构造方法
        /// </summary>
        static GenericExtensions()
        {
            entityInfoCache = new ConcurrentDictionary<string, Lazy<List<EntityInfo>>>();
        }

        /// <summary>
        /// 获取主键值
        /// </summary>
        /// <param name="t">对象</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPrimaryKeyValues<T>(this T t) where T : class
        {
            var type = typeof(T);
            var notMappedAttribute = typeof(NotMappedAttribute);
            var keyAttribute = typeof(KeyAttribute);
            var columnAttribute = typeof(ColumnAttribute);
            var keyValues = new Dictionary<string, object>();
            var propertyInfos = type.GetProperties().Where(w => w.IsDefined(keyAttribute));
            foreach (var item in propertyInfos)
            {
                keyValues.Add(item.IsDefined(columnAttribute) ? item.GetCustomAttribute<ColumnAttribute>().Name : item.Name, item.GetValue(t));
            }
            return keyValues;
        }

        /// <summary>
        /// 获取实体数据库映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">对象</param>
        /// <param name="parameterIndex">参数索引</param>
        /// <param name="filter">过滤</param>
        /// <returns></returns>
        public static EntityDbMapping GetEntityDbMapping<T>(this T t, int parameterIndex = 0, Expression<Func<PropertyInfo, bool>> filter = null) where T : class
        {
            var type = t.GetType();
            var cacheKey = $"{type.FullName}";
            if (filter != null)
            {
                cacheKey += filter.ToString();
            }
            var entityDbMapping = new EntityDbMapping();

            var cache = entityInfoCache.GetOrAdd(cacheKey, key => new Lazy<List<EntityInfo>>(() =>
            {
                var propertyInfos = type.GetProperties().Where(w => !w.IsDefined(typeof(NotMappedAttribute), false));
                if (filter != null)
                {
                    propertyInfos = propertyInfos.Where(filter.Compile());
                }
                return propertyInfos.Select(s => new EntityInfo()
                {
                    Property = s,
                    IsPrimaryKey = s.IsDefined(typeof(KeyAttribute), false),
                    ColumnName = s.IsDefined(typeof(ColumnAttribute)) ? s.GetCustomAttribute<ColumnAttribute>().Name : s.Name
                }).ToList();
            })).Value;

            parameterIndex++;

            entityDbMapping.EntityInfos = cache.Select(s =>
            {
                var info = new EntityInfo()
                {
                    Value = s.Property.GetValue(t),
                    IsPrimaryKey = s.IsPrimaryKey,
                    ColumnName = s.ColumnName
                };
                info.Identity = $"{info.ColumnName}_{parameterIndex}";
                entityDbMapping.DbParameters.Add(info.Identity, info.Value);
                return info;
            }).ToList();

            return entityDbMapping;
        }

        /// <summary>
        /// 获取实体列表数据库映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">对象列表</param>
        /// <param name="filter">过滤</param>
        /// <returns></returns>
        public static List<EntityDbMapping> GetEntityDbMappings<T>(this IEnumerable<T> list, Expression<Func<PropertyInfo, bool>> filter = null) where T : class
        {
            var entityDbMappings = new List<EntityDbMapping>();
            foreach (var item in list)
            {
                entityDbMappings.Add(item.GetEntityDbMapping(entityDbMappings.Count, filter));
            }
            return entityDbMappings;
        }

        /// <summary>
        /// 生成数据库参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源</param>
        /// <returns></returns>
        public static Dictionary<string, object> GenerateDbParameters<T>(this IEnumerable<T> source)
        {
            var dbParameters = new Dictionary<string, object>();
            foreach (var item in source)
            {
                dbParameters.Add(Guid.NewGuid().ToString().Replace("-", ""), item);
            }
            return dbParameters;
        }
    }
}

