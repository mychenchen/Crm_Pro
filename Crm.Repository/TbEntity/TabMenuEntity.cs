using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// tab导航菜单管理
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

        /// <summary>
        /// 路径
        /// </summary>
        [StringLength(50)]
        public string Location { get; set; }

        /// <summary>
        /// 菜单类型 1 PC端 2 小程序端 
        /// </summary>
        [Required]
        public int MenuType { get; set; }

        /// <summary>
        /// 是否显示 0否 1是 
        /// </summary>
        [Required]
        public int IsShow { get; set; }
    }
}
