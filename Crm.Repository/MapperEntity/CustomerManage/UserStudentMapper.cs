using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 学员表
    /// </summary>
    public class UserStudentMapper : BaseEntityMapper
    {
        /// <summary>
        /// 标签ID
        /// </summary>
        public Guid LableId { get; set; }

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
        /// 手机号
        /// </summary>
        public string Telephone { get; set; }

        /// <summary>
        /// 是否为VIP
        /// </summary>
        public int IsVIP { get; set; }

    }
}
