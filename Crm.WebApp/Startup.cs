using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Crm.Repository.DB;
using Crm.WebApp.AuthorizeHelper;
using Crm.WebApp.BasicsRegisterService;
using Crm.WebApp.Infrastructure;
using Currency.Common.LogManange;
using Currency.Common.SystemRegister;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            AllServiceRegister.DataBaseServiceRegister(services, Configuration);

            AllServiceRegister.BaseServiceRegister(services);

            AllServiceRegister.SwaggerServiceRegister(services);

            AllServiceRegister.SenparcWeixinServiceRegister(services, Configuration);


            var builder = new ContainerBuilder();
            builder.Populate(services);
            DI.Current = builder.Build();

            //启动mq消息接收程序
            //DI.GetService<IMqReceive>().ReceiveAll();

            Currency.Common.Redis.RedisHelperNetCore.Default.GetSubscribe("txt_demo");

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

            #region 开启后,可直接访问静态页面,静态文件
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
            #endregion

            #region Swagger设置

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelp V1");
            });
            #endregion

            app.UseCors("allow_all").UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
            });
            app.UseWebSockets();

            // 启动 CO2NET 全局注册，必须！
            IRegisterService register = RegisterService.Start(env, senparcSetting.Value)
                                                        .UseSenparcGlobal(false, null);

            //开始注册微信信息，必须！
            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value);

            LogHelper.Configure(); //使用前先配置

            // 添加默认数据
            var myContext = DI.GetService<MyDbContext>();
            DefaultDataSeed.SeedAsync(myContext).Wait();

            app.UseMvc();

        }
    }
}
