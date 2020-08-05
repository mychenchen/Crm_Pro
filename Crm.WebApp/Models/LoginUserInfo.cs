using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.WebApp.Models
{
    public class LoginUserPwd
    {
        public string account { get; set; }

        public string password { get; set; }
    }

    /// <summary>
    /// 登陆用户
    /// </summary>
    public class LoginUserInfo
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Gid { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 登陆账号
        /// </summary>
        public string LoginUser { get; set; }

        /// <summary>
        /// 登陆token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 登陆时间
        /// </summary>
        public DateTime LoginTime { get; set; }
    }
}
