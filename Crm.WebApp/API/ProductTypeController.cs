using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;
using Crm.Service.ProductManageService;
using Crm.WebApp.Infrastructure;
using Crm.WebApp.Models;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace Crm.WebApp.API
{
    /// <summary>
    /// 产品管理
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class ProductTypeController : ApiBaseController
    {
        protected readonly IProductTypeService _product;
        public ProductTypeController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IProductTypeService notice
            ) : base(configStr, mapper)
        {
            _product = notice;
        }

        /// <summary>
        /// 查询分类
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetAllSelect()
        {
            try
            {
                var data = _product.GetList();
                var list = RecursionTree.LayuiTreeList(data, Guid.Empty);

                return Success(list);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

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
                var list = _mapper.Map<List<ProductTypeMapper>>(data);

                return SuccessPage(page, limit, count, list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="pid">父级ID</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetDataByPid(Guid pid)
        {
            try
            {
                var data = _product.GetListByPid(pid);
                var list = _mapper.Map<List<ProductTypeMapper>>(data);

                return Success(list);
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
                var info = _mapper.Map<ProductTypeMapper>(data);

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
        public ResultObject SaveModel(ProductTypeMapper model)
        {
            var optEvent = "添加";
            var errStr = "成功";
            try
            {
                var saveEntity = _mapper.Map<ProductTypeEntity>(model);
                if (saveEntity.Id != Guid.Empty)
                {
                    optEvent = "修改";
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
                SaveUserOperation("ProductTypeController", optEvent, $"结果:{errStr})");
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject DeleteModel(Guid gid)
        {
            var errStr = "成功";
            try
            {
                _product.Delete(gid, true);
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
                SaveUserOperation("ProductTypeController", "删除", $"结果:{errStr}");
            }
        }

    }
}
