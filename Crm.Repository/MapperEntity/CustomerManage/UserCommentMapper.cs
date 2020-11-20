using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 用户评论
    /// </summary>
    public class UserCommentMapper : BaseEntityMapper
    {
        /// <summary>
        /// 回复评论Id
        /// </summary>
        public Guid ReplyId { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string CommentTxt { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

    }
}
