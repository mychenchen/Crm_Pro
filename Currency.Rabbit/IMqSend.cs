using System;
using System.Collections.Generic;
using System.Text;

namespace Currency.Mq
{
    public interface IMqSend
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="messageStr"></param>
        void SendByName(string queueName, string messageStr);

    }
}
