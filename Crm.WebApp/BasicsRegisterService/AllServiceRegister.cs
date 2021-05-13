using AutoMapper;
using Crm.Repository.DB;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Models;
using Currency.Common.SystemRegister;
using Currency.Mq.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin.RegisterServices;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;

namespace Crm.WebApp.BasicsRegisterService
{
    /// <summary>
    /// 服务注册
    /// </summary>
    public class AllServiceRegister
    {
        public static void BaseServiceRegister(IServiceCollection services)
        {

            //设置json数据返回
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //json字符串大小写原样输出
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });

            services.AddAutoMapper();

            services.AddAssembly("Crm.Service");

            services.AddSignalR();

            //单个注入 

            //services.AddSingleton<IMqSend, SendMessage>();
            //services.AddSingleton<IMqReceive, ReceiveMessage>();

            //接收mq的消息
            //services.AddSingleton<BatchHandle>();

            ////注入 MongoDB
            //services.AddSingleton<MongoDBService>();

            services.AddCors(options =>
            {
                options.AddPolicy("allow_all", q =>
                {
                    //buildler.WithOrigins("http://localhost:49554")
                    q
                    .SetIsOriginAllowed(origin => true)
                    //.AllowAnyOrigin() //允许任何来源的主机访问  SignalR 2.2不允许使用 
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();//指定处理cookie
                });
            });

            //全局拦截
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(AuthorizeAction));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc();

        }

        /// <summary>
        /// 数据库注册
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        public static void DataBaseServiceRegister(IServiceCollection services, IConfiguration Configuration)
        {
            //services.Configure<DataSettingsModel>(Configuration.GetSection("DataSettings")).AddMvc();
            services.Configure<CmsAppSettingModel>(Configuration.GetSection("CmsAppSetting")).AddMvc();
            services.Configure<RabbitBaseInfo>(Configuration.GetSection("RabbitSetting")).AddMvc();

            var connection = Configuration.GetConnectionString("SqlServer");

            services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(connection, b => b.MigrationsAssembly("Crm.Repository")));

            services.AddScoped<DefaultDataSeed>();

        }
        /// <summary>
        /// Swagger
        /// </summary>
        /// <param name="services"></param>
        public static void SwaggerServiceRegister(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Hello", Version = "v1" });
                //var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                //var xmlPath = Path.Combine(basePath, "CMS.Project.xml");
                //c.IncludeXmlComments(xmlPath);

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });


            services.AddMvcCore().AddApiExplorer();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        public static void SenparcWeixinServiceRegister(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                   .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        public static void CopyServiceRegister(IServiceCollection services, IConfiguration Configuration)
        {

        }

    }
}
