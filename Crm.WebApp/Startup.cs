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

            //����json���ݷ���
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //json�ַ�����Сдԭ�����
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });

            services.AddAutoMapper();

            services.AutoRegisterServicesFromAssembly("Crm.Service");
            //ע�� ����MemoryCacheManager ��ʵ���� IStaticCacheManager
            services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();

            //services.AddSingleton<IMqSend, SendMessage>();
            //services.AddSingleton<IMqReceive, ReceiveMessage>();

            //����mq����Ϣ
            //services.AddSingleton<BatchHandle>();



            ////ע�� redis
            //services.AddSingleton<RedisCacheHelper>();

            ////ע�� MongoDB
            //services.AddSingleton<MongoDBService>();


            services.AddCors(options =>
            {
                options.AddPolicy("allow_all", q =>
                {
                    //buildler.WithOrigins("http://localhost:49554")
                    q
                    .SetIsOriginAllowed(origin => true)
                    //.AllowAnyOrigin() //�����κ���Դ����������  SignalR 2.2������ʹ�� 
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();//ָ������cookie
                });
            });

            services.AddSignalR();

            //ȫ������
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

            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET ȫ��ע��
                   .AddSenparcWeixinServices(Configuration);//Senparc.Weixin ע��
            #endregion


            var builder = new ContainerBuilder();
            builder.Populate(services);
            DI.Current = builder.Build();

            //����mq��Ϣ���ճ���
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

            //������,��ֱ�ӷ��ʾ�̬ҳ��,��̬�ļ�
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

            // ���� CO2NET ȫ��ע�ᣬ���룡
            IRegisterService register = RegisterService.Start(env, senparcSetting.Value)
                                                        .UseSenparcGlobal(false, null);

            //��ʼע��΢����Ϣ�����룡
            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value);

            LogHelper.Configure(); //ʹ��ǰ������

            // ���Ĭ������
            var myContext = DI.GetService<MyDbContext>();
            DefaultDataSeed.SeedAsync(myContext).Wait();

            //QuartzService.StartJobs<QuartzJob>();  //������

        }
    }
}
