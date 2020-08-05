using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static Currency.Common.NetCoreDIModuleRegister;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户管理
    /// UseDI特性用于注入必须加
    /// </summary>
    [UseDI(ServiceLifetime.Scoped, typeof(IUserService))]
    public class UserService : IUserService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public UserService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<User> GetList()
        {
            return _mydb.User.ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<User> GetPageList(string name, int page, int rows, ref int count)
        {
            var list = _mydb.User.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.NickName.Contains(name));
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
        public User GetModel(Guid gid)
        {
            return _mydb.User.FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 根据登陆名称查询
        /// </summary>
        /// <param name="name"></param>
        public User UserLoginModel(string name)
        {
            return _mydb.User.FirstOrDefault(a => a.LoginName == name);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(User model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _mydb.User.Add(model);
            }
            else
            {
                _mydb.Update(model);
            }
            _mydb.SaveChanges();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        public bool Delete(Guid gid)
        {
            var model = GetModel(gid);
            if (model == null)
                return false;

            _mydb.User.Remove(model);
            _mydb.SaveChanges();
            return true;
        }

        #region 验证

        /// <summary>
        /// 验证登陆账号是否重复
        /// </summary>
        /// <param name="model"></param>
        public bool VerifyLoginName(Guid gid, string name)
        {
            var model = _mydb.User.FirstOrDefault(a => a.LoginName == name && a.Id != gid);

            return model == null ? false : true;
        }

        #endregion

    }
}
