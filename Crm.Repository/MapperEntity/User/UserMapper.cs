using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 系统用户
    /// </summary>
    [AutoMappers(typeof(User))]
    public class UserMapper : BaseEntityMapper
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public Guid LabelId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid RoleId { get; set; }

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
        /// 自我介绍
        /// </summary>
        public string MyIntroduce { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string HeadImg { get; set; }

    }
}
