using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 用户订单
    /// </summary>
    [Table("tb_UserOrder")]
    public class UserOrderEntity : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required, StringLength(50)]
        public string OrderCode { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        public Guid ProductId { get; set; }

        /// <summary>
        /// 商品标题
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// 商品图片
        /// </summary>
        [Required]
        [StringLength(200)]
        public string CoverPath { get; set; }

        /// <summary>
        /// 商品原价
        /// </summary>
        [Required]
        public double Price { get; set; }

        /// <summary>
        /// 折后价
        /// </summary>
        [Required]
        public double DiscountPrice { get; set; }

        /// <summary>
        /// 实际支付
        /// </summary>
        [Required]
        public double ActualPay { get; set; }

        /// <summary>
        /// 支付状态 0待支付 1已支付 2已取消
        /// </summary>
        [Required]
        public int PayState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(200)]
        public string Remarks { get; set; }

    }
}
