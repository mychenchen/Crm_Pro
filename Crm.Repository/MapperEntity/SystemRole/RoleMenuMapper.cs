﻿using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 角色菜单
    /// </summary>
    public class RoleMenuMapper : BaseEntityMapper
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 菜单Ids 
        /// </summary>
        public string MenuIds { get; set; }

    }
}
