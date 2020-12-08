using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 产品分类信息
    /// </summary>
    [AutoMappers(typeof(ProductTypeEntity))]
    public class ProductTypeMapper : BaseEntityMapper
    {
        /// <summary>
        /// 标题
        /// </summary>
        public Guid PID { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 分类编号
        /// </summary>
        public string TypeCode { get; set; }

    }
}
