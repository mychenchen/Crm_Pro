using Crm.Repository.DB;
using Currency.Common.LogManange;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Crm.Service.BaseHelper
{
    /// <summary>
    /// 实现通用接口仓库
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseServiceRepository<T> where T : class, new()
    {
        protected readonly MyDbContext myDbContext;
        public BaseServiceRepository(MyDbContext m)
        {
            this.myDbContext = m;
        }

        /// <summary>
        /// 扩展方法，自带方法不能满足的时候可以添加新方法
        /// </summary>
        /// <returns></returns>
        public int CommQuery(string json)
        {
            return myDbContext.Database.ExecuteSqlCommand(json);
        }

        #region 增

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public T Insert(T entity)
        {
            var model = myDbContext.Set<T>().Add(entity);
            myDbContext.SaveChanges();
            return model.Entity;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public void InsertArr(List<T> list)
        {
            myDbContext.Set<T>().AddRange(list);
            myDbContext.SaveChanges();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<T> InsertAsync(T entity)
        {
            var model = await myDbContext.Set<T>().AddAsync(entity);
            await myDbContext.SaveChangesAsync();
            return model.Entity;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task InsertArrAsync(List<T> list)
        {
            await myDbContext.Set<T>().AddRangeAsync(list);
            await myDbContext.SaveChangesAsync();
        }

        #endregion

        #region 删

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="t"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        /// <returns></returns>
        public bool Delete(T entity)
        {
            try
            {
                myDbContext.Set<T>().Remove(entity);
                myDbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("删除异常 \n " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="t"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        /// <returns></returns>
        public bool Delete(Expression<Func<T, bool>> whereLambda)
        {
            try
            {
                LogHelper.Error("删除异常 \n " + "测试一下");
                var list = myDbContext.Set<T>().Where(whereLambda);
                if (list.Any())
                {
                    myDbContext.Set<T>().RemoveRange(list);
                    myDbContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("删除异常 \n " + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 改

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="t"></param>
        public void Update(T entity)
        {
            myDbContext.Set<T>().Update(entity);
            myDbContext.SaveChanges();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="t"></param>
        public void Update(List<T> list)
        {
            myDbContext.Set<T>().UpdateRange(list);
            myDbContext.SaveChanges();
        }

        #endregion

        #region 查
        /// <summary>
        /// 判断是否存在符合条件的结果
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public bool IsExist(Expression<Func<T, bool>> whereLambda)
        {
            return myDbContext.Set<T>().Any(whereLambda);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public T GetEntity(Expression<Func<T, bool>> whereLambda)
        {
            return myDbContext.Set<T>().AsNoTracking().FirstOrDefault(whereLambda);
        }

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        public List<T> Select()
        {
            return myDbContext.Set<T>().ToList();
        }

        /// <summary>
        /// 根据条件查询所有
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<T> SelectWhere(Expression<Func<T, bool>> whereLambda)
        {
            return myDbContext.Set<T>().Where(whereLambda).ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">条数</param>
        /// <param name="whereLambda">条件</param>
        /// <param name="orderByLambda">排序</param>
        /// <param name="isAsc">是否正序 true-正序 false-反序</param>
        /// <returns></returns>
        public ServicePageResult<T> SelectPage<S>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, S>> orderByLambda, bool isAsc)
        {
            ServicePageResult<T> res = new ServicePageResult<T>
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            res.TotalSize = myDbContext.Set<T>().Where(whereLambda).Count();
            
            if (isAsc)
            {
                var entities = myDbContext.Set<T>().Where(whereLambda)
                                      .OrderBy<T, S>(orderByLambda)
                                      .Skip(pageSize * (pageIndex - 1))
                                      .Take(pageSize).ToList();
                res.List = entities;
                return res;
            }
            else
            {
                var entities = myDbContext.Set<T>().Where(whereLambda)
                                      .OrderByDescending<T, S>(orderByLambda)
                                      .Skip(pageSize * (pageIndex - 1))
                                      .Take(pageSize).ToList();
                res.List = entities;
                return res;
            }
        }

        #region 异步方法

        /// <summary>
        /// 判断是否存在符合条件的结果
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public async Task<bool> IsExistAsync(Expression<Func<T, bool>> whereLambda)
        {
            return await myDbContext.Set<T>().AnyAsync(whereLambda);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public async Task<T> GetEntityAsync(Expression<Func<T, bool>> whereLambda)
        {
            return await myDbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(whereLambda);
        }

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> SelectAsync()
        {
            return await myDbContext.Set<T>().ToListAsync();
        }

        /// <summary>
        /// 根据条件查询所有
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public async Task<List<T>> SelectWhereAsync(Expression<Func<T, bool>> whereLambda)
        {
            return await myDbContext.Set<T>().Where(whereLambda).ToListAsync();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="whereLambda"></param>
        /// <param name="orderByLambda"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public async Task<ServicePageResult<T>> SelectPageAsync<S>(int pageSize, int pageIndex, Expression<Func<T, bool>> whereLambda, Expression<Func<T, S>> orderByLambda, bool isAsc)
        {
            ServicePageResult<T> res = new ServicePageResult<T>
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            res.TotalSize = await myDbContext.Set<T>().Where(whereLambda).CountAsync();

            if (isAsc)
            {
                var entities = await myDbContext.Set<T>().Where(whereLambda)
                                      .OrderBy<T, S>(orderByLambda)
                                      .Skip(pageSize * (pageIndex - 1))
                                      .Take(pageSize).ToListAsync();
                res.List = entities;
                return res;
            }
            else
            {
                var entities = await myDbContext.Set<T>().Where(whereLambda)
                                      .OrderByDescending<T, S>(orderByLambda)
                                      .Skip(pageSize * (pageIndex - 1))
                                      .Take(pageSize).ToListAsync();
                res.List = entities;
                return res;
            }
        }

        #endregion

        #endregion
    }
}
