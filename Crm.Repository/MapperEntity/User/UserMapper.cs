using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class UserMapper : BaseEntityMapper
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 登陆账号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 登陆密码
        /// </summary>
        public string LoginPwd { get; set; }

        /// <summary>
        /// 加盐
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// 最近一次登陆时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 创建时间格式化
        /// </summary>        
        public string LastLoginTimeStr
        {
            get
            {
                return LastLoginTime == null ? "" : LastLoginTime.Value.ToString("yyyy-MM-dd HH:mm:ss");

            }
        }

    }
}
