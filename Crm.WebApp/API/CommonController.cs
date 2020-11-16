﻿using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Service.GatewayService;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Models;
using Currency.Common.Caching;
using Currency.Common.LogManange;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        [HttpPost, NoSign]
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
                    DeleteImage(wuliPath + oldPath);
                }
                return Success(url);
            }
            catch (Exception ex)
            {
                DeleteImage(wuliPath + url);
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 富文本编辑框,图片上传
        /// </summary>
        /// <returns></returns>
        [HttpPost, NoSign]
        public async Task<IActionResult> WangEditorUpload()
        {
            try
            {
                List<string> list = new List<string>();
                var files = Request.Form.Files;
                var wuliPath = Directory.GetCurrentDirectory();
                string uploadPath = "uploads" + "/" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        string fileExt = Path.GetExtension(formFile.FileName).Trim('.'); //文件扩展名，不含“.”
                        string newFileName = Guid.NewGuid().ToString().Replace("-", "") + "." + fileExt; //随机生成新的文件名

                        var filePath = Path.Combine(uploadPath, newFileName);

                        using (var stream = new FileStream(filePath, FileMode.CreateNew))
                        {
                            await formFile.CopyToAsync(stream);
                            var url = $@"/{uploadPath}/{newFileName}";
                            list.Add(url);
                        }
                    }
                }
                return new JsonResult(list);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                return new JsonResult(ex.Message);
            }

        }

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="url"></param>
        private void DeleteImage(string url)
        {
            try
            {
                System.IO.File.Delete(url);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        #endregion

        #region 上传文件

        /// <summary>
        /// 上传 文件,并返回相对url(不包含 host port wwwroot)
        /// 上传旧图地址,则会删除旧图
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, NoSign]
        public async Task<ResultObject> UploadFileWj(upModel model)
        {
            if (model.file == null)
            {
                return Error("请先选择文件");
            }
            var wuliPath = Directory.GetCurrentDirectory();
            //文件夹名称 model.fileName
            //页码 model.page
            //总页码 model.totalPage
            //格式 model.fileExt 
            string uploadPath = "uploads" + "/" + model.fileName;
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            string newFileName = model.fileName + "#" + model.page; //随机生成新的文件名

            var filePath = Path.Combine(uploadPath, newFileName);
            var url = $@"/{uploadPath}/{newFileName}";
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.file.CopyToAsync(stream);
                }
                if (model.page != model.totalPage)
                    return Success(1, "继续上传", url);
                else
                {
                    //合并文件
                    url = FileMerge(uploadPath, model.fileName);
                    return Success(2, "上传完成", url);
                }
            }
            catch (Exception ex)
            {
                DeleteImage(wuliPath + url);
                return Error(ex.Message);
            }
        }
        /// <summary>
        /// 文件合并
        /// </summary>
        /// <param name="filePath">文件夹路径</param>
        /// <param name="fullName">存放路径</param>
        private string FileMerge(string filePath, string fullName)
        {
            var wuliPath = Directory.GetCurrentDirectory();
            if (!Directory.Exists(wuliPath + filePath))
            {
                throw new Exception("文件不存在");
            }

            var files = Directory.GetFiles(wuliPath + filePath);

            if (!(files.Length > 0))
            {
                throw new Exception("文件列表为空");
            }
            List<fileList> list_s = new List<fileList>();
            foreach (var item in files)
            {
                var sp_ = item.Split('#');
                list_s.Add(new fileList
                {
                    nameNum = int.Parse(sp_[1]),
                    path = item
                });
            }
            list_s = list_s.OrderBy(a => a.nameNum).ToList();

            byte[] buffer = new byte[1024 * 100];
            using (FileStream outStream = new FileStream(wuliPath + fullName, FileMode.Create))
            {
                int readedLen = 0;
                FileStream srcStream = null;

                list_s.ForEach(a =>
                {
                    srcStream = new FileStream(a.path, FileMode.Open);
                    while ((readedLen = srcStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outStream.Write(buffer, 0, readedLen);
                    }
                    srcStream.Close();
                });
            }
            return fullName;
        }

        public class fileList
        {
            public int nameNum { get; set; }

            public string path { get; set; }
        }

        /// <summary>
        /// 文件分片上传
        /// </summary>
        public class upModel
        {
            /// <summary>
            /// 文件流
            /// </summary>
            public IFormFile file { get; set; }

            /// <summary>
            /// 文件名称
            /// </summary>
            public string fileName { get; set; }

            /// <summary>
            /// 当前分片数
            /// </summary>
            public string page { get; set; }

            /// <summary>
            /// 总分片数
            /// </summary>
            public string totalPage { get; set; }

            /// <summary>
            /// 文件后缀名
            /// </summary>
            public string fileExt { get; set; }
        }

        #endregion

    }
}
