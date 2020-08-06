using Crm.Repository.TbEntity;
using System;
using System.Collections.Generic;

namespace Crm.Service.GatewayService
{
    /// <summary>
    /// tab菜单
    /// </summary>
    public interface ITabMenuService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<TabMenuEntity> GetList();

        /// <summary>
        /// 查询子级菜单
        /// </summary>
        /// <param name="pid"></param>
        List<TabMenuEntity> GetListByPid(Guid pid);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<TabMenuEntity> GetPageList(string name, int page, int rows, ref int count);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        TabMenuEntity GetModel(Guid gid);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        void AddUpdateModel(TabMenuEntity model);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="isDelete">true 真删除 false 假删除</param>
        bool Delete(Guid gid, bool isDelete = false);

    }
}
