using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Service.GatewayService;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Models;
using Currency.Common.Caching;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Crm.WebApp.API
{
    /// <summary>
    /// 常用函数接口
    /// </summary>
    [EnableCors("allow_all")]
    [Route("api/[controller]/[action]")]
    public class CommonController : ApiBaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ITabMenuService _tabMenu;
        public CommonController(
            IOptions<CmsAppSettingModel> configStr,
            IMapper mapper,
            IStaticCacheManager cache,
            IHostingEnvironment hostingEnvironment,
            ITabMenuService tabMenu
            ) : base(configStr, mapper, cache)
        {
            _hostingEnvironment = hostingEnvironment;
            _tabMenu = tabMenu;
        }

        #region 下拉菜单

        /// <summary>
        /// 查询tab菜单
        /// </summary>
        /// <param name="pid">父级ID</param>
        /// <returns></returns>
        [HttpGet]
        public ResultObject GetAllTabMenu(Guid pid)
        {
            try
            {
                var data = _tabMenu.GetListByPid(pid);
                var list = _mapper.Map<List<TabMenuMapper>>(data);

                return Success(list);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        #endregion

        #region 上传图片

        /// <summary>
        /// 上传 文件,并返回相对url(不包含 host port wwwroot)
        /// 上传旧图地址,则会删除旧图
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost,NoSign]
        public async Task<ResultObject> UploadFile(IFormFile file)
        {
            if (file == null)
            {
                return Error("请先选择文件");
            }
            var wuliPath = Directory.GetCurrentDirectory();
            var fileName = Request.Query["fileName"];
            var oldPath = Request.Query["oldPath"];
            string uploadPath = "uploads" + "/" + DateTime.Now.ToString("yyyyMMdd");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string fileExt = Path.GetExtension(file.FileName).Trim('.'); //文件扩展名，不含“.”
            string newFileName = Guid.NewGuid().ToString().Replace("-", "") + "." + fileExt; //随机生成新的文件名
            //是否存在自定义名称
            if (!string.IsNullOrEmpty(fileName))
            {
                newFileName = fileName + "." + fileExt;
            }
            var filePath = Path.Combine(uploadPath, newFileName);
            var url = $@"/{uploadPath}/{newFileName}";
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(oldPath))
                {
                    System.IO.File.Delete(wuliPath + oldPath);
                }
                return Success(url);
            }
            catch (Exception ex)
            {
                System.IO.File.Delete(wuliPath + url);
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 富文本框上传图片
        /// </summary>
        /// <param name="action"></param>
        /// <param name="noCache"></param>
        /// <returns></returns>
        [HttpGet, NoSign]
        public ResultObject UEditorUploadFile(string action, string callback)
        {
            try
            {
                return Success();
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        #endregion

    }
}
