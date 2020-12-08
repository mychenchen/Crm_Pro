using Crm.Repository.DB;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 学员管理
    /// </summary>
    public class UserStudentService : IUserStudentService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public UserStudentService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<UserStudentEntity> GetList()
        {
            return _mydb.UserStudent.ToList();
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
            var list = from a in _mydb.UserStudent
                       join b in _mydb.UserLabel on a.LabelId equals b.Id into temp
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

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public UserStudentEntity GetModel(Guid gid)
        {
            return _mydb.UserStudent.AsNoTracking().FirstOrDefault(a => a.Id == gid);
        }

        /// <summary>
        /// 根据登陆名称查询
        /// </summary>
        /// <param name="name"></param>
        public UserStudentEntity UserLoginModel(string name)
        {
            return _mydb.UserStudent.AsNoTracking().FirstOrDefault(a => a.LoginName == name);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        public void AddUpdateModel(UserStudentEntity model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _mydb.UserStudent.Add(model);
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
                _mydb.UserStudent.Remove(model);
            else
            {
                model.IsDelete = 1;
                _mydb.UserStudent.Update(model);
            }
            _mydb.SaveChanges();
            return true;
        }

    }
}
