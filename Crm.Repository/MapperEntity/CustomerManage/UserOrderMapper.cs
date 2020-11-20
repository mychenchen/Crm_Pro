using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 用户订单
    /// </summary>
    public class UserOrderMapper : BaseEntityMapper
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// 商品标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 商品图片
        /// </summary>
        public string CoverPath { get; set; }

        /// <summary>
        /// 商品原价
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// 折后价
        /// </summary>
        public double DiscountPrice { get; set; }

        /// <summary>
        /// 实际支付
        /// </summary>
        public double ActualPay { get; set; }

        /// <summary>
        /// 支付状态 0待支付 1已支付 2已取消
        /// </summary>
        public int PayState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

    }
}
