using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;

namespace Fast.Framework.Aop
{

    /// <summary>
    /// 动态代理工厂
    /// </summary>
    public static class DynamicProxyFactory
    {

        /// <summary>
        /// 缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, Lazy<object>> cache;

        /// <summary>
        /// 构造方法
        /// </summary>
        static DynamicProxyFactory()
        {
            cache = new ConcurrentDictionary<string, Lazy<object>>();
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">目标</param>
        /// <param name="intercept">拦截器</param>
        /// <returns></returns>
        public static T Create<T>(object target, Intercept intercept)
        {
            var key = typeof(T).GUID.ToString();
            var obj = cache.GetOrAdd(key, k => new Lazy<object>(() =>
            {
                return DispatchProxy.Create<T, DynamicProxy<T>>();
            }));
            var proxy = obj.Value as DynamicProxy<T>;
            proxy.target = target;
            proxy.intercept = intercept;
            return (T)(proxy as object);
        }
    }
}
