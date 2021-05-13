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

            //����mq��Ϣ���ճ���
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

            #region ������,��ֱ�ӷ��ʾ�̬ҳ��,��̬�ļ�
            app.UseStaticFiles();
            string upl = Directory.GetCurrentDirectory() + @"/uploads";
            if (!Directory.Exists(upl))
            {
                Directory.CreateDirectory(upl);
            }
            //�Զ�����Դ
            app.UseStaticFiles(new StaticFileOptions
            {
                //��Դ���ڵľ���·����
                FileProvider = new PhysicalFileProvider(upl),
                //��ʾ����·��,����'/'��ͷ
                RequestPath = "/uploads"
            });

            app.UseMvcWithDefaultRoute();
            #endregion

            #region Swagger����

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

            // ���� CO2NET ȫ��ע�ᣬ���룡
            IRegisterService register = RegisterService.Start(env, senparcSetting.Value)
                                                        .UseSenparcGlobal(false, null);

            //��ʼע��΢����Ϣ�����룡
            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value);

            LogHelper.Configure(); //ʹ��ǰ������

            // ���Ĭ������
            var myContext = DI.GetService<MyDbContext>();
            DefaultDataSeed.SeedAsync(myContext).Wait();

            app.UseMvc();

        }
    }
}
