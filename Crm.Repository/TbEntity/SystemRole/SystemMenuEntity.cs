using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [Table("tb_SystemMenu")]
    public class SystemMenuEntity : BaseEntity
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
        public string Name { get; set; }

        /// <summary>
        /// 图标 
        /// </summary>
        [StringLength(200)]
        public string Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Required]
        public int SortNum { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        [StringLength(50)]
        public string Location { get; set; }

        /// <summary>
        /// 是否显示 0否 1是 
        /// </summary>
        [Required]
        public int IsShow { get; set; }

    }
}
