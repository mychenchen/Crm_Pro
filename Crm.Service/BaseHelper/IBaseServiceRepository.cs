using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Crm.Service.BaseHelper
{
    /// <summary>
    /// 通用接口仓库
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseServiceRepository<T> where T : class, new()
    {
        #region 增

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        T Insert(T t);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        void InsertArr(List<T> t);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<T> InsertAsync(T t);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task InsertArrAsync(List<T> t);

        #endregion

        #region 删

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool Delete(T t);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool Delete(Expression<Func<T, bool>> whereLambda);

        #endregion

        #region 改

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="t"></param>
        void Update(T t);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="t"></param>
        void Update(List<T> t);

        #endregion

        #region 查

        /// <summary>
        /// 判断是否存在符合条件的结果
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        bool IsExist(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        T GetEntity(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        List<T> Select();

        /// <summary>
        /// 根据条件查询所有
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        List<T> SelectWhere(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="pageSize">条数</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="whereLambda">条件</param>
        /// <param name="orderByLambda">排序</param>
        /// <param name="isAsc">是否正序</param>
        /// <returns></returns>
        ServicePageResult<T> SelectPage<S>(int pageSize, int pageIndex, Expression<Func<T, bool>> whereLambda, Expression<Func<T, S>> orderByLambda, bool isAsc);

        #region 异步方法

        /// <summary>
        /// 判断是否存在符合条件的结果
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        Task<bool> IsExistAsync(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        Task<T> GetEntityAsync(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        Task<List<T>> SelectAsync();

        /// <summary>
        /// 根据条件查询所有
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        Task<List<T>> SelectWhereAsync(Expression<Func<T, bool>> whereLambda);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="pageSize">条数</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="whereLambda">条件</param>
        /// <param name="orderByLambda">排序</param>
        /// <param name="isAsc">是否正序</param>
        /// <returns></returns>
        Task<ServicePageResult<T>> SelectPageAsync<S>(int pageSize, int pageIndex, Expression<Func<T, bool>> whereLambda, Expression<Func<T, S>> orderByLambda, bool isAsc);

        #endregion

        #endregion
    }
}
