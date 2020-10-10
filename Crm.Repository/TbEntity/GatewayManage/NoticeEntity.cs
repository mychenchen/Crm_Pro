using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 公告管理
    /// </summary>
    [Table("tb_Notice")]
    public class NoticeEntity : BaseEntity
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        /// <summary>
        /// 公告类型
        /// </summary>
        public Guid NoticeType { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required]
        public string NewsContent { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public string FileDownload { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
    }
}
