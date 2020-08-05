using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static Currency.Common.NetCoreDIModuleRegister;

namespace Crm.Service.SystemService
{
    /// <summary>
    /// 用户操作日志
    /// UseDI特性用于注入必须加
    /// </summary>
    [UseDI(ServiceLifetime.Scoped, typeof(IOperationLogService))]
    public class OperationLogService : IOperationLogService
    {
        /// <summary>
        /// 数据库
        /// </summary>
        protected readonly MyDbContext _mydb;

        public OperationLogService(MyDbContext mydb)
        {
            _mydb = mydb;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        public List<OperationLogEntity> GetList()
        {
            return _mydb.OperationLog.ToList();
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
        public List<OperationLogEntity> GetPageList(string optEvent,string controllerStr, int page, int rows, ref int count)
        {
            var list = _mydb.OperationLog.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(optEvent))
            {
                list = list.Where(a => a.OperationEvent == optEvent);
            }
            if (!string.IsNullOrEmpty(optEvent))
            {
                list = list.Where(a => a.OpentionControllerStr.Contains(controllerStr));
            }
            var data = list.OrderByDescending(a => a.CreateTime)
                .Skip((page - 1) * rows).Take(rows).ToList();
            count = list.Count();
            return data;
        }

        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="model"></param>
        public void SaveLog(OperationLogEntity model)
        {
            _mydb.OperationLog.Add(model);
            _mydb.SaveChanges();
        }
    }
}
