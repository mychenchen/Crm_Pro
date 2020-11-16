﻿using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.ProductManageService;
using Crm.WebApp.Models;
using Currency.Common.Caching;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 产品管理
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class ProductController : ApiBaseController
    {
        protected readonly IProductService _product;
        public ProductController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IStaticCacheManager cache,
            IProductService notice
            ) : base(configStr, mapper, cache)
        {
            _product = notice;
        }

        #region 后台服务接口

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="title">名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetData(string title, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _product.GetPageList(title, page, limit, ref count);
                var list = _mapper.Map<List<ProductMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 查询详情,根据ID
        /// </summary>
        /// <param name="gid">名称</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetDetailByGid(Guid gid)
        {
            try
            {
                var data = _product.GetModel(gid);
                var info = _mapper.Map<ProductMapper>(data);

                return Success(info);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 保存或修改
        /// </summary>
        /// <param name="model">产品</param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject SaveModel(ProductMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<ProductEntity>(model);
                var entity = _product.GetModel(saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    optEvent = "修改";
                }
                else
                {
                    model.CreateTime = DateTime.Now;
                    model.FloorPrice = model.Price;
                    model.HotNum = 0;
                    model.OnShelfStatus = 0;
                    model.IssueDateTime = DateTime.Now;
                }

                _product.AddUpdateModel(saveEntity);
                return Success();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                errStr = "失败,请查看日志";
                return Error(errStr);
            }
            finally
            {
                SaveUserOperation("ProductController", optEvent, $"添加产品,结果:{errStr})");
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject DeleteModel(string gid)
        {
            var errStr = "成功";
            try
            {
                _product.Delete(Guid.Parse(gid));
                return Success();
            }
            catch (Exception ex)
            {
                errStr = "失败,请查看日志";
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
            finally
            {
                SaveUserOperation("ProductController", "删除", $"删除公告,结果:{errStr}");
            }
        }

        /// <summary>
        /// 产品发行
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="onShelf"> 1上架 2下架</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject ProductIssue(string gid, int onShelf)
        {
            var errStr = "成功";
            try
            {
                _product.UpdateOnShelf(Guid.Parse(gid), onShelf);
                return Success();
            }
            catch (Exception ex)
            {
                errStr = "失败,请查看日志";
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
            finally
            {
                SaveUserOperation("ProductController", "删除", $"删除公告,结果:{errStr}");
            }
        }

        #region 课时视频

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="gid">名称</param>
        /// <param name="page">页码</param>
        /// <param name="limit">每页条数</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetVideoData(string gid, int page, int limit)
        {
            try
            {
                var count = 0;
                var data = _product.GetVideoList(Guid.Parse(gid), page, limit, ref count);
                var list = _mapper.Map<List<ProductVideoMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 保存或修改
        /// </summary>
        /// <param name="model">课时</param>
        /// <returns></returns>
        [HttpPost]
        public ResultObject SaveVideoModel(ProductVideoMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<ProductVideoEntity>(model);
                var entity = _product.GetVideoModel(saveEntity.Id);
                if (entity != null)
                {
                    saveEntity.CoverPath = "";
                    saveEntity.TypeName = "";
                    saveEntity.CreateTime = entity.CreateTime;
                    saveEntity.IsDelete = entity.IsDelete;
                    optEvent = "修改";
                }
                else
                {
                    model.CreateTime = DateTime.Now;
                    model.CoverPath = "";
                    model.TypeName = "0000";
                }

                _product.AddUpdateVideoModel(saveEntity);
                return Success();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                errStr = "失败,请查看日志";
                return Error(errStr);
            }
            finally
            {
                SaveUserOperation("ProductController", optEvent, $"添加产品,结果:{errStr})");
            }
        }

        #endregion

        #endregion

    }
}