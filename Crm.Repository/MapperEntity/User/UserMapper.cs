using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 系统用户
    /// </summary>
    public class UserMapper : BaseEntityMapper
    {
        /// <summary>
        /// 标签名称
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

    }
}
