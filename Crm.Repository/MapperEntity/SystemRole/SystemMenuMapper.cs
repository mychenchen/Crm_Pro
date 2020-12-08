using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [AutoMappers(typeof(SystemMenuEntity))]
    public class SystemMenuMapper : BaseEntityMapper
    {
        /// <summary>
        /// 父级ID
        /// </summary>
        public Guid ParentGid { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 图标 
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortNum { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 是否显示 0否 1是 
        /// </summary>
        public int IsShow { get; set; }

    }
}
