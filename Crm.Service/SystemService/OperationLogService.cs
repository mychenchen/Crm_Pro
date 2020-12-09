using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;
using System.Collections.Generic;
using System.Linq;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户操作日志
    /// </summary>
    public class OperationLogService : BaseServiceRepository<OperationLogEntity>, IOperationLogService
    {

        public OperationLogService(MyDbContext mydb) : base(mydb)
        {
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="optEvent">类型</param>
        /// <param name="controllerStr">模块</param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<OperationLogEntity> GetPageList(string optEvent, string controllerStr, int page, int rows, ref int count)
        {
            var list = myDbContext.OperationLog.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(optEvent))
            {
                list = list.Where(a => a.OperationEvent == optEvent);
            }
            if (!string.IsNullOrEmpty(controllerStr))
            {
                list = list.Where(a => a.OpentionControllerStr.Contains(controllerStr));
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

    }
}
