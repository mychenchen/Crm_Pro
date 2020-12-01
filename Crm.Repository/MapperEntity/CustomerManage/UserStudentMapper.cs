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
        public Guid LabelId { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public string LabelName { get; set; }

        /// <summary>
        /// 标签图片
        /// </summary>
        public string LabelImgPath { get; set; }

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

        /// <summary>
        /// 当前登陆
        /// </summary>
        public string RecentlyIp { get; set; }

    }
}
