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

        /// <summary>
        /// 排序
        /// </summary>
        public int SortNum { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 菜单类型 1 PC端 2 小程序端 
        /// </summary>
        public int MenuType { get; set; }

        /// <summary>
        /// 是否显示 0否 1是 
        /// </summary>
        public int IsShow { get; set; }
    }
}
