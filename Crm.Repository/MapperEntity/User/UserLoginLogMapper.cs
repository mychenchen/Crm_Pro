using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 用户登陆记录表
    /// </summary>
    [AutoMappers(typeof(UserLoginLog))]
    public class UserLoginLogMapper : BaseEntityMapper
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Ip
        /// </summary>
        public string Ip { get; set; }

    }
}
