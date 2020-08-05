using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 操作日志表
    /// </summary>
    [Table("tb_OperationLog")]
    public class OperationLogEntity : BaseEntity
    {
        /// <summary>
        /// 操作人
        /// </summary>
        [Required]
        public string OperationUser { get; set; }

        /// <summary>
        /// 操作的模块
        /// </summary>
        public string OpentionControllerStr { get; set; }

        /// <summary>
        /// 操作事件 1添加 2修改 3删除
        /// </summary>
        public string OperationEvent { get; set; }

        /// <summary>
        /// 操作内容 
        /// </summary>
        public string OpentionContext { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperationTime { get; set; }
    }
}
