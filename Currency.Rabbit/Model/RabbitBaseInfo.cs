using System.Collections.Generic;

namespace Currency.Mq.Model
{
    /// <summary>
    /// Rabbit通用参数
    /// </summary>
    public class RabbitBaseInfo
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string UserName { set; get; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 队列名称集合
        /// </summary>
        public List<string> QueueNameList { get; set; }
    }

}
