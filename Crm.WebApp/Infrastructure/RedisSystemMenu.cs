using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Service.SystemService;
using Currency.Common.Redis;
using Currency.Common.SystemRegister;
using System;
using System.Collections.Generic;

namespace Crm.WebApp.Infrastructure
{
    /// <summary>
    /// 获取系统菜单缓存
    /// </summary>
    public class RedisSystemMenu
    {
        //单例模式
        public static Redis_SystemMenu Default
        {
            get
            {
                return new Redis_SystemMenu();
            }
        }

        public class Redis_SystemMenu
        {
            protected static ISystemMenuService _sysMenu;
            protected static IMapper _mapper;

            public Redis_SystemMenu()
            {
                _sysMenu = DI.GetService<ISystemMenuService>();
                _mapper = DI.GetService<IMapper>();
            }
            /// <summary>
            /// 获取所有菜单
            /// </summary>
            /// <returns></returns>
            public List<SystemMenuMapper> GetRedisMenuList()
            {
                string key = "all_system_menu";
                var list = RedisHelperNetCore.Default.GetKey<List<SystemMenuMapper>>(key);
                if (list == null || list.Count == 0)
                {
                    list = ReloadMenuList();
                }
                return list;
            }

            /// <summary>
            /// 保存最新
            /// </summary>
            /// <returns></returns>
            public List<SystemMenuMapper> ReloadMenuList()
            {
                string key = "all_system_menu";
                RedisHelperNetCore.Default.KeyDelete(key);
                var data = _sysMenu.SelectWhere(a => a.IsDelete == 0);
                var list = _mapper.Map<List<SystemMenuMapper>>(data);
                RedisHelperNetCore.Default.SetStringKey(key, list, TimeSpan.FromDays(7));
                return list;
            }
        }
    }
}
