using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 新闻管理
    /// </summary>
    [Table("tb_HotNews")]
    public class HotNewsEntity : BaseEntity
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        [StringLength(100)]
        public string Subtitle { get; set; }

        /// <summary>
        /// 信息来源
        /// </summary>
        [StringLength(50)]
        public string InformationSource { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required]
        public string NewsContent { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// 发布时间(到时间才显示,否则不显示)
        /// </summary>
        public DateTime? ShowTime { get; set; }

    }
}
