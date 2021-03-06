﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 产品信息
    /// </summary>
    [Table("tb_Product")]
    public class ProductEntity : BaseEntity
    {
        /// <summary>
        /// 产品分类
        /// </summary>
        [Required]
        public Guid ProductTypeId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [Required]
        [StringLength(200)]
        public string CoverPath { get; set; }

        /// <summary>
        /// 底价(不展示在前台)
        /// </summary>
        [Required]
        public double FloorPrice { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        [Required]
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
        [Required]
        public DateTime IssueDateTime { get; set; }

        /// <summary>
        /// 状态 0 未上架 1已上架 2下架 
        /// </summary>
        [Required]
        public int OnShelfStatus { get; set; }

        /// <summary>
        /// 是否热门,正序热门(数字越小越靠前)
        /// </summary>
        public int HotNum { get; set; }
    }
}
