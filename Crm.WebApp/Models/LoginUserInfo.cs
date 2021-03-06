﻿using System;

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
        public Guid Gid { get; set; }

        /// <summary>
        /// 唯一ID
        /// </summary>
        public Guid RoleId { get; set; }
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

        /// <summary>
        /// 是否在线 
        /// </summary>
        public bool OnLine { get; set; }

        /// <summary>
        /// 是否在线 
        /// </summary>
        public string OnLineStr
        {
            get
            {
                if (OnLine)
                    return "在线";
                else
                    return "离线";
            }
        }
        /// <summary>
        /// 连接ID
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string HeadImg { get; set; }
    }
}
