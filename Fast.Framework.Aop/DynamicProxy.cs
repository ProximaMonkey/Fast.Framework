using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fast.Framework.Aop
{

    /// <summary>
    /// 动态代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DynamicProxy<T> : DispatchProxy
    {
        /// <summary>
        /// 目标
        /// </summary>
        public object target;

        /// <summary>
        /// 拦截器
        /// </summary>
        public Intercept intercept;

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        protected override object Invoke(MethodInfo methodInfo, object[] args)
        {
            object result = null;
            if (intercept.Where(target, methodInfo, args))
            {
                intercept.Before(target, methodInfo, args);
                if (methodInfo.ReturnType.FullName.Equals("System.Threading.Tasks.Task"))
                {
                    var task = methodInfo.Invoke(target, args) as dynamic;
                    if (methodInfo.ReturnType.GenericTypeArguments.Length == 0)
                    {
                        task.Wait();
                        result = task;
                    }
                    else
                    {
                        result = Task.FromResult(task.Result);
                    }
                }
                else
                {
                    result = methodInfo.Invoke(target, args);
                }
                intercept.After(target, methodInfo, args);
            }
            else
            {
                result = methodInfo.Invoke(target, args);
            }
            return result;
        }
    }
}
