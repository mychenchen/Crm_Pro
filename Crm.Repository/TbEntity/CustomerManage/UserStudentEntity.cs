using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 学员表
    /// </summary>
    [Table("tb_UserStudent")]
    public class UserStudentEntity : BaseEntity
    {
        /// <summary>
        /// 昵称
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NickName { get; set; }

        /// <summary>
        /// 登陆账号
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LoginName { get; set; }

        /// <summary>
        /// 登陆密码
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LoginPwd { get; set; }

        /// <summary>
        /// 加盐
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Salt { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Telephone { get; set; }

        /// <summary>
        /// 是否为VIP 0 否 1是
        /// </summary>
        [Required]
        public int IsVIP { get; set; }

        /// <summary>
        /// 标签ID
        /// </summary>
        public Guid LableId { get; set; }

    }
}
