using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 公告管理
    /// </summary>
    [AutoMappers(typeof(NoticeEntity))]
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
