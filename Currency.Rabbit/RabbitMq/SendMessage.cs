using Currency.Common;
using Currency.Mq.Model;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Currency.Mq.RabbitMq
{
    /// <summary>
    /// Rabbit消息管理
    /// </summary>
    public class SendMessage : IMqSend
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly RabbitBaseInfo info;
        public SendMessage(IOptions<RabbitBaseInfo> iop)
        {
            info = iop.Value;
            //创建连接工厂对象
            IConnectionFactory conFactory = new ConnectionFactory
            {
                HostName = info.HostName, //IP地址
                Port = info.Port, //端口号
                UserName = info.UserName, //用户账号
                Password = info.Password //用户密码
            };
            this.connection = conFactory.CreateConnection();
            this.channel = connection.CreateModel();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="messageStr"></param>
        public void SendByName(string queueName, string messageStr)
        {
            //声明一个队列
            channel.QueueDeclare(
              queue: queueName,//消息队列名称
              durable: false,//是否缓存
              exclusive: false,
              autoDelete: false,
              arguments: null
               );
            //消息内容
            byte[] body = Encoding.UTF8.GetBytes(messageStr);
            //发送消息
            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }


    }
}
