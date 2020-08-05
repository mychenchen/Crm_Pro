using Quartz;
using Quartz.Spi;
using System;

namespace Currency.Quartz
{
    /// <summary>
    /// 任务队列工厂
    /// </summary>
    public class QuartzFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public QuartzFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;

            var job = (IJob)_serviceProvider.GetService(jobDetail.JobType);
            return job;
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}
