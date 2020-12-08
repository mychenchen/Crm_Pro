using Crm.Repository.DB;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserService : BaseServiceRepository<User>, IUserService
    {
        public UserService(MyDbContext mydb) : base(mydb)
        {
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
            var list = from a in myDbContext.User
                       join b in myDbContext.UserLabel on a.LabelId equals b.Id into temp
                       from bb in temp.DefaultIfEmpty()
                       where a.IsDelete == 0
                       select new UserMapper
                       {
                           Id = a.Id,
                           CreateTime = a.CreateTime,
                           UpdateTime = a.UpdateTime,
                           IsDelete = a.IsDelete,
                           Salt = a.Salt,
                           LabelId = bb.Id,
                           LabelImgPath = bb.ImgPath,
                           LabelName = bb.LabelName,
                           LoginName = a.LoginName,
                           LoginPwd = a.LoginPwd,
                           NickName = a.NickName,
                           RoleId = a.RoleId,
                           Sex = a.Sex,
                           MyIntroduce = a.MyIntroduce,
                           HeadImg = a.HeadImg
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

    }
}
