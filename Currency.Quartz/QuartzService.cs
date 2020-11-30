using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Currency.Quartz
{
    /// <summary>
    /// 任务服务
    /// </summary>
    public static class QuartzService
    {
        #region 单任务

        /// <summary>
        /// 单个任务
        /// </summary>
        /// <typeparam name="TJob">实现类</typeparam>
        /// <param name="key"></param>
        /// <param name="secondsTime">执行间隔 / 秒</param>
        /// <param name="triggerList">自定义参数</param>
        public static void StartJob<TJob>(string key, int secondsTime, Dictionary<string, string> triggerList) where TJob : IJob
        {
            //通过工场类获得调度器
            var scheduler = new StdSchedulerFactory().GetScheduler().Result;

            //Jobs即我们需要执行的作业
            var jobB = JobBuilder.Create<TJob>()
              .WithIdentity(key);

            triggerList.ToList().ForEach(a =>
            {
                jobB.UsingJobData(a.Key, a.Value);
            });
            IJobDetail job = jobB.Build();

            //创建触发器(也叫时间策略)
            var trigger1 = TriggerBuilder.Create()
              .WithIdentity(key + ".trigger")
              .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(secondsTime)).RepeatForever())
              .ForJob(job)
              .StartNow()
              .Build();

            scheduler.AddJob(job, true);

            //将触发器和作业任务绑定到调度器中
            scheduler.ScheduleJob(job, trigger1);

            //开启调度器
            scheduler.Start();
        }
        #endregion

        #region 多任务
        public static void StartJobs<TJob>() where TJob : IJob
        {
            var scheduler = new StdSchedulerFactory().GetScheduler().Result;

            var job = JobBuilder.Create<TJob>()
              .WithIdentity("第一个", "同一个组")
              .Build();

            var trigger1 = TriggerBuilder.Create()
              .WithIdentity("jobWork1")
              .StartNow()
              .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(5)).RepeatForever())
              .ForJob(job)
              .Build();

            var trigger2 = TriggerBuilder.Create()
              .WithIdentity("jobWork2")
              .StartNow()
              .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(11)).RepeatForever())
              .ForJob(job)
              .Build();

            var dictionary = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>
            {
                {job, new HashSet<ITrigger> {trigger1, trigger2}}
            };
            scheduler.ScheduleJobs(dictionary, true);
            scheduler.Start();

        }
        #endregion

        #region 配置
        public static void AddQuartz(this IServiceCollection services, params Type[] jobs)
        {
            services.AddSingleton<IJobFactory, QuartzFactory>();
            services.Add(jobs.Select(jobType => new ServiceDescriptor(jobType, jobType, ServiceLifetime.Singleton)));

            services.AddSingleton(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                scheduler.Start();
                return scheduler;
            });
        }
        #endregion
    }
}
