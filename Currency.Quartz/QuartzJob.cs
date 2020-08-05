using Currency.Common.LogManange;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Currency.Quartz
{
    /// <summary>
    /// 处理job任务
    /// </summary>
    public class QuartzJob : IJob
    {
        /// <summary>
        /// 处理作业
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;//获取job信息
            var triggerKey = context.Trigger.Key;//获取trigger信息   --jobWork1  --jobWork2

            switch (triggerKey.Name)
            {
                case "jobWork1":
                    LogHelper.Info(" 这里进行处理 ‘第一个’ 工作任务");
                    break;
                case "jobWork2":
                    LogHelper.Info(" 这里进行处理 ‘第二个’ 工作任务");
                    break;
                default:

                    break;
            }

            await Task.CompletedTask;
        }
    }
}
