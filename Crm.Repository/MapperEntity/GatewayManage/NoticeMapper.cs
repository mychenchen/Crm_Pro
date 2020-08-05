using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 公告管理
    /// </summary>
    public class NoticeMapper : BaseEntityMapper
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 公告类型
        /// </summary>
        public Guid NoticeType { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string NewsContent { get; set; }

    }
}
