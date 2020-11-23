using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    public interface ISystemMenuService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<SystemMenuEntity> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<SystemMenuEntity> GetPageList(string name, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        SystemMenuEntity GetModel(Guid gid);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateModel(SystemMenuEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool Delete(Guid gid, bool isDelete = false);

    }
}
