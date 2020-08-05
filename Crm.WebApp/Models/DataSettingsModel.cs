using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.WebApp.Models
{
    /// <summary>
    /// 连接字符串类
    /// </summary>
    public class DataSettingsModel
    {
        /// <summary>
        /// 读库
        /// </summary>
        public string ConnRead { get; set; }

        /// <summary>
        /// 写库
        /// </summary>
        public string ConnWrite { get; set; }

        /// <summary>
        /// 日志库
        /// </summary>
        public string ConnLog { get; set; }
    }
}
