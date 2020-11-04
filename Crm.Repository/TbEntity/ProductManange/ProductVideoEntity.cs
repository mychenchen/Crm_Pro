using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    [Table("tb_ProductVideo")]
    public class ProductVideoEntity : BaseEntity
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        [Required]
        public Guid ProId { get; set; }

        /// <summary>
        /// 所属类别
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TypeName { get; set; }

        /// <summary>
        /// 视频名称
        /// </summary>
        [Required]
        [StringLength(50)]
        public string VideoName { get; set; }

        /// <summary>
        /// 大小(自动计算)
        /// </summary>
        public string VideoSize { get; set; }

        /// <summary>
        /// 时长  
        /// </summary>
        [Required]
        [StringLength(30)]
        public string TimeLength { get; set; }

        /// <summary>
        /// 视频地址
        /// </summary>
        [Required]
        [StringLength(200)]
        public string VideoPath { get; set; }
    }
}
