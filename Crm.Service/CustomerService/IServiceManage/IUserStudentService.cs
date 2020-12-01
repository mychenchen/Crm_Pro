using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户学生
    /// </summary>
    public interface IUserStudentService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<UserStudentEntity> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isVip"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<UserStudentMapper> GetMapperPageList(string name, int isVip, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        UserStudentEntity GetModel(Guid gid);

        /// <summary>
        /// 根据登陆名称查询
        /// </summary>
        /// <param name="name"></param>
        UserStudentEntity UserLoginModel(string name);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateModel(UserStudentEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool Delete(Guid gid, bool isDelete = false);

    }
}
