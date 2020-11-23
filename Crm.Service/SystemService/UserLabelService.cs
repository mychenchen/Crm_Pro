using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static Currency.Common.NetCoreDIModuleRegister;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户标签
    /// UseDI特性用于注入必须加
    /// </summary>
    [UseDI(ServiceLifetime.Scoped, typeof(IUserLabelService))]
    public class UserLabelService : IUserLabelService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public UserLabelService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<UserLabel> GetList()
        {
            return _mydb.UserLabel.ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<UserLabel> GetPageList(string name, int page, int rows, ref int count)
        {
            var list = _mydb.UserLabel.Where(a => a.IsDelete == 0);

            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.LabelName.Contains(name));
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public UserLabel GetModel(Guid gid)
        {
            return _mydb.UserLabel.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(UserLabel model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _mydb.UserLabel.Add(model);
            }
            else
            {
                _mydb.UserLabel.Update(model);
            }
            _mydb.SaveChanges();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        public bool Delete(Guid gid, bool isDelete = false)
        {
            var model = GetModel(gid);
            if (model == null)
                return false;
            if (isDelete)
                _mydb.UserLabel.Remove(model);
            else
            {
                model.IsDelete = 1;
                _mydb.UserLabel.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

    }
}
