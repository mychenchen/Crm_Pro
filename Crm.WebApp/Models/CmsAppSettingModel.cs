using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.WebApp.Models
{
    /// <summary>
    /// 系统全局字符串
    /// </summary>
    public class CmsAppSettingModel
    {
        /// <summary>
        /// 是否为测试环境
        /// </summary>
        public bool IsDebug { get; set; } = false;

    }
}
