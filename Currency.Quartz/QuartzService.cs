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
        public static void StartJob<TJob>() where TJob : IJob
        {
            var scheduler = new StdSchedulerFactory().GetScheduler().Result;

            var job = JobBuilder.Create<TJob>()
              .WithIdentity("job")
              .Build();

            var trigger1 = TriggerBuilder.Create()
              .WithIdentity("job.trigger")
              .StartNow()
              .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(5)).RepeatForever())
              .ForJob(job)
              .Build();

            scheduler.AddJob(job, true);
            scheduler.ScheduleJob(job, trigger1);
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
