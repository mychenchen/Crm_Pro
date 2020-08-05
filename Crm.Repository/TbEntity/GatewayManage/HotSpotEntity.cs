using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 热点轮播
    /// </summary>
    [Table("tb_HotSpot")]
    public class HotSpotEntity : BaseEntity
    {
        /// <summary>
        /// 图片标题
        /// </summary>
        [StringLength(50)]
        public string ImgTitle { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ImgPath { get; set; }

        /// <summary>
        /// 内容地址
        /// </summary>
        [StringLength(200)]
        public string ContentUrl { get; set; }
    }
}
