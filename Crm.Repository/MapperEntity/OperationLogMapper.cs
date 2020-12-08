using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 操作日志表
    /// </summary>
    [AutoMappers(typeof(OperationLogEntity))]
    public class OperationLogMapper : BaseEntityMapper
    {
        /// <summary>
        /// 操作人
        /// </summary>
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
