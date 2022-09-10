using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Interfaces
{

    /// <summary>
    /// 插入接口类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInsert<T>
    {
        /// <summary>
        /// 插入构建
        /// </summary>
        IInsertBuilder InsertBuilder { get; }

        /// <summary>
        /// 作为
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        IInsert<T> As(string tableName);

        /// <summary>
        /// 执行异步
        /// </summary>
        /// <returns></returns>
        Task<int> ExceuteAsync();

        /// <summary>
        /// 执行返回自增ID异步
        /// </summary>
        /// <returns></returns>
        Task<int> ExceuteReturnIdentityAsync();
    }
}
