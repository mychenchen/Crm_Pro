using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 产品信息
    /// </summary>
    [AutoMappers(typeof(ProductEntity))]
    public class ProductMapper : BaseEntityMapper
    {
        /// <summary>
        /// 产品分类
        /// </summary>
        public Guid ProductTypeId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverPath { get; set; }

        /// <summary>
        /// 底价(不展示在前台)
        /// </summary>
        public double FloorPrice { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public double Discount { get; set; }

        /// <summary>
        /// 折后价(存在折扣时展示)
        /// </summary>
        public double DiscountPrice { get; set; }

        /// <summary>
        /// 产品内容
        /// </summary>
        public string ProductContent { get; set; }

        /// <summary>
        /// 发行时间
        /// </summary>
        public DateTime IssueDateTime { get; set; }

        /// <summary>
        /// 状态 0 未上架 1已上架 
        /// </summary>
        public int OnShelfStatus { get; set; }

        /// <summary>
        /// 是否热门,正序热门(数字越小越靠前)
        /// </summary>
        public int HotNum { get; set; }
    }
}
