using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 用户评论
    /// </summary>
    [Table("tb_UserComment")]
    public class UserCommentEntity : BaseEntity
    {
        /// <summary>
        /// 回复评论Id
        /// </summary>
        [Required]
        public Guid ReplyId { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required]
        public string CommentTxt { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

    }
}
