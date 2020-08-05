using AutoMapper;
using Crm.WebApp.Models;
using Crm.Repository.MapperEntity;
using Crm.Service.SystemService;
using Currency.Common.Caching;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Crm.WebApp.API
{
    /// <summary>
    /// 系统管理
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class SystemManageController : ApiBaseController
    {
        protected readonly IUserLoginLogService _userLog;
        protected readonly IOperationLogService _optLog;
        public SystemManageController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IStaticCacheManager cache,
            IUserLoginLogService userLog,
            IOperationLogService optLog
            ) : base(configStr, mapper, cache)
        {

            _userLog = userLog;
            _optLog = optLog;
        }

        #region 系统用户查询

        /// <summary>
        /// 系统用户列表
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetLoginLogData(string name, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _userLog.GetPageList(name, page, limit, ref count);
                var list = _mapper.Map<List<UserLoginLogMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        #endregion

        #region 用户操作日志

        /// <summary>
        /// 系统用户列表
        /// </summary>
        /// <param name="optEvent">类型</param>
        /// <param name="controllerStr">模块名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetOptLogData(string optEvent, string controllerStr, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _optLog.GetPageList(optEvent, controllerStr, page, limit, ref count);
                var list = _mapper.Map<List<UserLoginLogMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }


        #endregion
    }
}
