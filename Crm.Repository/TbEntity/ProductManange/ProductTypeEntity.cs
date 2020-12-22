using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 产品分类信息
    /// </summary>
    [Table("tb_ProductType")]
    public class ProductTypeEntity : BaseEntity
    {
        /// <summary>
        /// 父ID
        /// </summary>
        [Required]
        public Guid PID { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        [Required]
        [StringLength(30)]
        public string TypeName { get; set; }

        /// <summary>
        /// 分类编号
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TypeCode { get; set; }

    }
}
