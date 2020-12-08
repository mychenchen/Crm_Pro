using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 产品视频
    /// </summary>
    [AutoMappers(typeof(ProductVideoEntity))]
    public class ProductVideoMapper : BaseEntityMapper
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public Guid ProId { get; set; }

        /// <summary>
        /// 所属类别
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 视频名称
        /// </summary>
        public string VideoName { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverPath { get; set; }

        /// <summary>
        /// 大小(自动计算)
        /// </summary>
        public string VideoSize { get; set; }

        /// <summary>
        /// 时长  
        /// </summary>
        public string TimeLength { get; set; }

        /// <summary>
        /// 视频地址
        /// </summary>
        public string VideoPath { get; set; }
    }
}
