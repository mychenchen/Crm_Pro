using AutoMapper;
using Crm.Service.GatewayService;
using Crm.Service.SystemService;
using Crm.WebApp.Models;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using System.Linq;
using Crm.WebApp.AuthorizeHelper;
using Crm.Service.ProductManageService;

namespace Crm.WebApp.API.WebApp
{
    /// <summary>
    /// 首页
    /// </summary>
    [Route("api/WebApp/[controller]/[action]")]
    [EnableCors("allow_all")]
    public class HomeCenterController : ApiBaseController
    {
        protected readonly IHotSpotService _hot;
        protected readonly IProductService _pro;
        protected readonly IProductTypeService _proType;

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        /// <param name="hot"></param>
        /// <param name="pro"></param>
        /// <param name="proType"></param>
        public HomeCenterController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
             IHotSpotService hot,
             IProductService pro,
             IProductTypeService proType
            ) : base(configStr, mapper)
        {
            _hot = hot;
            _pro = pro;
            _proType = proType;

        }

        /// <summary>
        /// 热点轮播图
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public async Task<ResultObject> HotSpot()
        {
            try
            {
                var list = await _hot.SelectWhereAsync(a => a.IsDelete == 0);
                var data = list.Select(a => new
                {
                    Id = a.Id,
                    Title = a.ImgTitle,
                    a.ImgPath,
                    a.ContentUrl
                }).ToList();
                return Success(data);
            }
            catch (Exception ex)
            {
                LogHelper.Error("轮播图 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 产品分类
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public async Task<ResultObject> ProductTypeList()
        {
            try
            {
                var data = await _proType.SelectWhereAsync(a => a.IsDelete == 0);
                var list = data.Select(a => new
                {
                    a.Id,
                    a.TypeName
                }).ToList();

                return Success(list);
            }
            catch (Exception ex)
            {
                LogHelper.Error("产品视频分类 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }


        /// <summary>
        /// 产品视频分类查询
        /// </summary>
        /// <param name="typePid">类型ID</param>
        /// <param name="pageSize">条数默认4条</param>
        /// <returns></returns>
        [HttpGet, NoSign]
        public async Task<ResultObject> ProductTypeVideo(Guid typePid, int pageSize = 4)
        {
            try
            {
                var typeList = await _proType.SelectWhereAsync(a => a.PID == typePid && a.IsDelete == 0);
                var tIds = typeList.Select(a => a.Id).ToArray();
                var proList = await _pro.SelectPageAsync(pageSize, 1,
                    a => tIds.Contains(a.ProductTypeId) && a.IsDelete == 0 && a.OnShelfStatus == 1,
                    a => a.IssueDateTime,
                    false);

                if (proList.TotalSize > 0) {
                    var list = proList.List.Select(a => new
                    {
                        a.Id,
                        a.Price,
                        a.DiscountPrice,
                        a.CoverPath,
                        a.IssueDateTime,
                        a.Subtitle,
                        a.Title
                    }).ToList();
                    return Success(list);
                }
                return Success();
            }
            catch (Exception ex)
            {
                LogHelper.Error("产品视频分类 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

    }
}
