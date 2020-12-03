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
        /// 自我介绍
        /// </summary>
        public string MyIntroduce { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string SexStr
        {
            get
            {
                if (Sex == 1)
                    return "男";
                else if (Sex == 0)
                    return "女";
                else
                    return "未设置";
            }
        }

        /// <summary>
        /// 头像
        /// </summary>
        public string HeadImg { get; set; }

    }
}
