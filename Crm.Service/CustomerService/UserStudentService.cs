using Crm.Repository.DB;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.CustomerService
{
    /// <summary>
    /// 学员管理
    /// </summary>
    public class UserStudentService : BaseServiceRepository<UserStudentEntity>, IUserStudentService
    {
        public UserStudentService(MyDbContext mydb) : base(mydb)
        {
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isVip"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<UserStudentMapper> GetMapperPageList(string name, int isVip, int page, int rows, ref int count)
        {
            var list = from a in myDbContext.UserStudent
                       join b in myDbContext.UserLabel on a.LabelId equals b.Id into temp
                       from bb in temp.DefaultIfEmpty()
                       where a.IsDelete == 0
                       select new UserStudentMapper
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
                           NickName = a.NickName,
                           IsVIP = a.IsVIP,
                           Telephone = a.Telephone,
                           Sex = a.Sex,
                           MyIntroduce = a.MyIntroduce,
                           HeadImg = a.HeadImg
                       };

            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.NickName.Contains(name));
            }
            if (isVip > -1)
            {
                list = list.Where(a => a.IsVIP == isVip);
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

    }
}
