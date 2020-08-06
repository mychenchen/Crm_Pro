using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// tab菜单管理
    /// </summary>
    [Table("tb_TabMenu")]
    public class TabMenuEntity : BaseEntity
    {
        /// <summary>
        /// 父级ID
        /// </summary>
        [Required]
        public Guid ParentGid { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set;}

        /// <summary>
        /// 排序
        /// </summary>
        [Required]
        public int SortNum { get; set; }

    }
}
