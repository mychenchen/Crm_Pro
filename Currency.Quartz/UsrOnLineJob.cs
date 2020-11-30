using Currency.Common.Redis;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Currency.Quartz
{
    /// <summary>
    /// 用户在线
    /// </summary>
    public class UsrOnLineJob : IJob
    {
        /// <summary>
        /// 处理作业
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var jobData = context.JobDetail.JobDataMap;//获取Job中的参数

            //var triggerData = context.Trigger.JobDataMap;//获取Trigger中的参数

            //var data = context.MergedJobDataMap;//获取Job和Trigger中合并的参数

            var red_1 = RedisHelperNetCore.Default.GetKey<int>("online_" + jobData.GetString("userId"));
            if (red_1 > 0)
            {
                RedisHelperNetCore.Default.SetStringKey("online_" + jobData.GetString("userId"), red_1 + 1, TimeSpan.FromMinutes(5));
            }

            await Task.CompletedTask;
        }
    }
}
