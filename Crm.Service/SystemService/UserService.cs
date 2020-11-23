using Crm.Repository.DB;
using Crm.Repository.MapperEntity;
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
        public List<UserMapper> GetMapperPageList(string name, int page, int rows, ref int count)
        {
            var list = from a in _mydb.User
                       join b in _mydb.UserLabel on a.LabelId equals b.Id into temp
                       from bb in temp.DefaultIfEmpty()
                       where a.IsDelete == 0
                       select new UserMapper
                       {
                           Id = a.Id,
                           CreateTime = a.CreateTime,
                           IsDelete = a.IsDelete,
                           Salt = a.Salt,
                           LabelId = bb.Id,
                           LabelImgPath = bb.ImgPath,
                           LabelName = bb.LabelName,
                           LoginName = a.LoginName,
                           LoginPwd = a.LoginPwd,
                           NickName = a.NickName
                       };

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
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        public bool Delete(Guid gid, bool isDelete = false)
        {
            var model = GetModel(gid);
            if (model == null)
                return false;
            if (isDelete)
                _mydb.User.Remove(model);
            else
            {
                model.IsDelete = 1;
                _mydb.User.Update(model);
            }
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
            var model = _mydb.User.FirstOrDefault(a => a.IsDelete == 0 && a.LoginName == name && a.Id != gid);

            return model == null ? false : true;
        }

        #endregion

    }
}
