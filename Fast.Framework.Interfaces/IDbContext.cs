using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace Fast.Framework.Interfaces
{

    /// <summary>
    /// 数据库上下文接口类
    /// </summary>
    public interface IDbContext : ITenant
    {
        /// <summary>
        /// Ado
        /// </summary>
        public IAdo Ado { get; }

        /// <summary>
        /// Aop
        /// </summary>
        public IAop Aop { get; }

        #region 增 删 改
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        IInsert<T> Insert<T>(T entity) where T : class;

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entitys">实体</param>
        /// <returns></returns>
        IInsert<T> Insert<T>(List<T> entitys) where T : class;

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="dictionary">字典</param>
        /// <returns></returns>
        IInsert<T> Insert<T>(Dictionary<string, object> dictionary) where T : class;

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="dictionarys">字典</param>
        /// <returns></returns>
        IInsert<T> Insert<T>(List<Dictionary<string, object>> dictionarys) where T : class;

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDelete<T> Delete<T>() where T : class;

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        IDelete<T> Delete<T>(T entity) where T : class;

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        IUpdate<T> Update<T>(T entity) where T : class;

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys">实体</param>
        /// <returns></returns>
        IUpdate<T> Update<T>(List<T> entitys) where T : class;

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary">字典</param>
        /// <returns></returns>
        IUpdate<T> Update<T>(Dictionary<string, object> dictionary) where T : class;

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionarys">字典</param>
        /// <returns></returns>
        IUpdate<T> Update<T>(List<Dictionary<string, object>> dictionarys) where T : class;
        #endregion

        #region 查询

        /// <summary>
        /// 子查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query">查询对象</param>
        /// <returns></returns>
        TResult SubQuery<TResult>(IQuery query);

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQuery<T> Query<T>() where T : class;

        /// <summary>
        /// 联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象数组</param>
        /// <returns></returns>
        IQuery<T> Union<T>(params IQuery<T>[] querys);

        /// <summary>
        /// 联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象列表</param>
        /// <returns></returns>
        IQuery<T> Union<T>(List<IQuery<T>> querys);

        /// <summary>
        /// 全联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象数组</param>
        /// <returns></returns>
        IQuery<T> UnionAll<T>(params IQuery<T>[] querys);

        /// <summary>
        /// 全联合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querys">查询对象列表</param>
        /// <returns></returns>
        IQuery<T> UnionAll<T>(List<IQuery<T>> querys);
        #endregion
    }
}
