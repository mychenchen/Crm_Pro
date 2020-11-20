using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Crm.Repository.DB;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.Models;
using Currency.Common;
using Currency.Common.Caching;
using Currency.Common.LogManange;
using Currency.Mq.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;

namespace Crm.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.Configure<DataSettingsModel>(Configuration.GetSection("DataSettings")).AddMvc();
            services.Configure<CmsAppSettingModel>(Configuration.GetSection("CmsAppSetting")).AddMvc();
            services.Configure<RabbitBaseInfo>(Configuration.GetSection("RabbitSetting")).AddMvc();

            //services.AddQuartz(typeof(QuartzJob));

            var connection = Configuration.GetConnectionString("SqlServer");

            services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(connection, b => b.MigrationsAssembly("Crm.Repository")));

            services.AddScoped<DefaultDataSeed>();

            //设置json数据返回
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //json字符串大小写原样输出
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });

            services.AddAutoMapper();

            services.AutoRegisterServicesFromAssembly("Crm.Service");
            //注入 碰到MemoryCacheManager 就实例给 IStaticCacheManager
            services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();

            //services.AddSingleton<IMqSend, SendMessage>();
            //services.AddSingleton<IMqReceive, ReceiveMessage>();

            //接收mq的消息
            //services.AddSingleton<BatchHandle>();



            ////注入 redis
            //services.AddSingleton<RedisCacheHelper>();

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

            services.AddSignalR();

            //全局拦截
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(AuthorizeLogin));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc();

            #region Swagger

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

            #endregion

            #region Senparc.Weixin

            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                   .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册
            #endregion


            var builder = new ContainerBuilder();
            builder.Populate(services);
            DI.Current = builder.Build();

            //启动mq消息接收程序
            //DI.GetService<IMqReceive>().ReceiveAll();


            return new AutofacServiceProvider(DI.Current);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseMvc();

            //开启后,可直接访问静态页面,静态文件
            app.UseStaticFiles();
            string upl = Directory.GetCurrentDirectory() + @"/uploads";
            if (!Directory.Exists(upl))
            {
                Directory.CreateDirectory(upl);
            }
            //自定义资源
            app.UseStaticFiles(new StaticFileOptions
            {
                //资源所在的绝对路径。
                FileProvider = new PhysicalFileProvider(upl),
                //表示访问路径,必须'/'开头
                RequestPath = "/uploads"
            });

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelp V1");
            });


            //app.UseCors("allow_all").UseSignalR(routes =>
            //{
            //    routes.MapHub<ChatHub>("/chatHub");
            //});
            //app.UseWebSockets();

            // 启动 CO2NET 全局注册，必须！
            IRegisterService register = RegisterService.Start(env, senparcSetting.Value)
                                                        .UseSenparcGlobal(false, null);

            //开始注册微信信息，必须！
            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value);

            LogHelper.Configure(); //使用前先配置

            // 添加默认数据
            var myContext = DI.GetService<MyDbContext>();
            DefaultDataSeed.SeedAsync(myContext).Wait();

            //QuartzService.StartJobs<QuartzJob>();  //多任务

        }
    }
}
