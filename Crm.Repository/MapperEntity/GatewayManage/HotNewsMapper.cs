using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 新闻管理
    /// </summary>
    [AutoMappers(typeof(HotNewsEntity))]
    public class HotNewsMapper : BaseEntityMapper
    {
        /// <summary>
        /// 父级菜单
        /// </summary>
        public Guid ParentGid { get; set; }

        /// <summary>
        /// 子级菜单
        /// </summary>
        public Guid ChildrenGid { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 信息来源
        /// </summary>
        public string InformationSource { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
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
