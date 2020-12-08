using Crm.Repository.TbEntity;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 热点轮播
    /// </summary>
    [AutoMappers(typeof(HotSpotEntity))]
    public class HotSpotMapper : BaseEntityMapper
    {
        /// <summary>
        /// 图片标题
        /// </summary>
        
        public string ImgTitle { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImgPath { get; set; }

        /// <summary>
        /// 内容地址
        /// </summary>
        public string ContentUrl { get; set; }
    }
}
