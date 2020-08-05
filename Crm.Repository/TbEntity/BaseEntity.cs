using System;
using System.ComponentModel.DataAnnotations;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 通用字段
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否删除 0否 1是
        /// </summary>
        public int IsDelete { get; set; } = 0;
    }
}
