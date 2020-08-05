using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// tab菜单管理
    /// </summary>
    public class TabMenuMapper : BaseEntityMapper
    {
        /// <summary>
        /// 父级ID
        /// </summary>
        public Guid ParentGid { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

    }
}
