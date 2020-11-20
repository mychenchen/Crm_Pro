using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 用户角色
    /// </summary>
    [Table("tb_UserRole")]
    public class UserRoleEntity : BaseEntity
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }

        /// <summary>
        /// 角色描述 
        /// </summary>
        [StringLength(200)]
        public string RoleDescribe { get; set; }

    }
}
