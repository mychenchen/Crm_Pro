using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 用户登陆记录表
    /// </summary>
    [Table("UserLoginLog")]
    public class UserLoginLog : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Ip
        /// </summary>
        public string Ip { get; set; }

    }
}
