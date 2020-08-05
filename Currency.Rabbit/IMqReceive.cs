using System;
using System.Collections.Generic;
using System.Text;

namespace Currency.Mq
{
    public interface IMqReceive
    {
        /// <summary>
        /// 获取mq消息
        /// </summary>
        void ReceiveAll();

    }
}
