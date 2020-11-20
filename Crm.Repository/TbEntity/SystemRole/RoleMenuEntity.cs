using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 角色菜单
    /// </summary>
    [Table("tb_RoleMenu")]
    public class RoleMenuEntity : BaseEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required]
        public Guid RoleId { get; set; }

        /// <summary>
        /// 菜单Ids 
        /// </summary>
        public string MenuIds { get; set; }

    }
}
