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
using Crm.Repository.TbEntity;
using System.Collections.Generic;

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
        protected readonly IUserService _user;

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="configStr"></param>
        /// <param name="mapper"></param>
        /// <param name="hot"></param>
        /// <param name="pro"></param>
        /// <param name="proType"></param>
        /// <param name="user"></param>
        public HomeCenterController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
             IHotSpotService hot,
             IProductService pro,
             IProductTypeService proType,
             IUserService user
            ) : base(configStr, mapper)
        {
            _hot = hot;
            _pro = pro;
            _proType = proType;
            _user = user;

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
        /// 教师卡片
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoSign]
        public async Task<ResultObject> TeacherList()
        {
            try
            {
                Guid rId = Guid.Parse("2e527bd2-d2d3-451f-908f-366620561408");
                var data = await _user.SelectWhereAsync(a => a.IsDelete == 0 && a.LabelId == rId);
                var list = data.Select(a => new
                {
                    a.HeadImg,
                    a.NickName,
                    a.MyIntroduce
                }).ToList();

                return Success(list);
            }
            catch (Exception ex)
            {
                LogHelper.Error("教师卡片 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 产品分类
        /// </summary>
        /// <param name="typePid">类型ID</param>
        /// <returns></returns>
        [HttpGet, NoSign]
        public async Task<ResultObject> ProductTypeList(Guid typePid)
        {
            try
            {
                var data = await _proType.SelectWhereAsync(a => a.IsDelete == 0 && a.PID == typePid);
                var list = data.Select(a => new
                {
                    a.Id,
                    a.TypeName
                }).ToList();

                return Success(list);
            }
            catch (Exception ex)
            {
                LogHelper.Error("产品分类 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }


        /// <summary>
        /// 课程视频分类查询
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

                if (proList.TotalSize > 0)
                {
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

        /// <summary>
        /// 课程搜索
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">条数</param>
        /// <param name="searchName">搜索名称</param>
        /// <param name="typePid">分类Id</param>
        /// <param name="isMoney">是否收费 -1所有 1收费 0免费</param>
        /// <returns></returns>
        [HttpGet, NoSign]
        public ResultObject ProductSearch(int pageIndex, int pageSize, string searchName, Guid typePid, int isMoney)
        {
            try
            {
                int count = 0;
                var proList = _pro.WebAppGetPageList(searchName, typePid, isMoney, pageIndex, pageSize, ref count)
                    .Select(a => new
                    {
                        a.Id,
                        a.Price,
                        a.DiscountPrice,
                        a.CoverPath,
                        a.IssueDateTime,
                        a.Subtitle,
                        a.Title
                    }).ToList();

                return Success(new { list = proList, count });
            }
            catch (Exception ex)
            {
                LogHelper.Error("产品视频分类 =>>" + ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 课程详情
        /// </summary>
        /// <param name="id">课程ID</param>
        /// <returns></returns>
        [HttpGet, NoSign]
        public async Task<ResultObject> ProductDetail(Guid id)
        {
            try
            {
                var info = await _pro.GetEntityAsync(a => a.Id == id);
                int count = 0;
                var list = _pro.GetVideoList(id, 1, 1, ref count);
                var detail = new
                {
                    info.Id,
                    info.Price,
                    info.DiscountPrice,
                    info.CoverPath,
                    info.ProductContent,
                    info.Title,
                    info.Subtitle,
                    VideoSum = count
                };
                return Success(detail);
            }
            catch (Exception ex)
            {
                LogHelper.Error("课程详情" + ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 课程视频列表
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet, NoSign]
        public ResultObject GetVideoPage(Guid proId, int pageIndex, int pageSize)
        {
            try
            {
                int count = 0;
                var list = _pro.GetVideoList(proId, pageIndex, pageSize, ref count);

                return Success(new { list, count });
            }
            catch (Exception ex)
            {
                LogHelper.Error("课程视频列表" + ex.ToString());
                return Error(ex.Message);
            }

        }

    }
}
