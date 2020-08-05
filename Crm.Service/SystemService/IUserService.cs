using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 系统用户接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<User> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<User> GetPageList(string name, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        User GetModel(Guid gid);

        /// <summary>
        /// 根据登陆名称查询
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        User UserLoginModel(string name);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateModel(User model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        bool Delete(Guid gid);

        #region 验证

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool VerifyLoginName(Guid gid, string name);

        #endregion
    }
}
