using Crm.Repository.TbEntity;
using System.Collections.Generic;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户操作日志
    /// </summary>
    public interface IOperationLogService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <returns></returns>
        List<OperationLogEntity> GetList();

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="optEvent">类型</param>
        /// <param name="controllerStr">模块</param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<OperationLogEntity> GetPageList(string optEvent, string controllerStr, int page, int rows, ref int count);

        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="model"></param>
        void SaveLog(OperationLogEntity model);
    }
}
