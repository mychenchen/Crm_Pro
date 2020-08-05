using Crm.Service.RabbitService;
using Currency.Common;
using Currency.Mq.Model;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Currency.Mq.RabbitMq
{
    public class ReceiveMessage : IMqReceive
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly RabbitBaseInfo info;

        public ReceiveMessage(IOptions<RabbitBaseInfo> iop)
        {
            info = iop.Value;
            IConnectionFactory conFactory = new ConnectionFactory
            {
                HostName = info.HostName, //IP地址
                Port = info.Port, //端口号
                UserName = info.UserName, //用户账号
                Password = info.Password //用户密码
            };
            connection = conFactory.CreateConnection();
            channel = connection.CreateModel();
        }

        /// <summary>
        /// 获取mq消息
        /// </summary>
        public void ReceiveAll()
        {
            info.QueueNameList.ForEach(queueName =>
            {
                channel.QueueDeclare(
                             queue: queueName,//消息队列名称
                             durable: false,//是否缓存
                             exclusive: false,
                             autoDelete: false,
                             arguments: null
                              );
                //创建消费者对象
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    byte[] message = ea.Body.ToArray();//接收到的消息
                    Process(queueName, Encoding.UTF8.GetString(message));
                };
                //消费者开启监听
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            });
        }

        /// <summary>
        /// 处理消息的方法
        /// </summary>
        /// <param name="message"></param>
        protected void Process(string queueName, string message)
        {
            var bh = DI.GetService<BatchHandle>();
            switch (queueName)
            {
                case "hotNews":
                    bh.SaveHotNews(message);
                    break;
                default:
                    break;
            }
        }


    }
}
