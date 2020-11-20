
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Crm.Repository.TbEntity;

namespace Crm.Repository.DB
{
    /// <summary>
    /// 默认数据添加
    /// </summary>
    public class DefaultDataSeed
    {
        /// <summary>
        /// 异步添加种子文件()
        /// </summary>
        /// <param name="myContext"></param>
        /// <returns></returns>
        public static async Task SeedAsync(MyDbContext myContext)
        {
            //初始化系统管理员账号
            if (!await myContext.User.AsQueryable().AnyAsync())
            {
                await myContext.User.AddAsync(
                   new User()
                   {
                       CreateTime = DateTime.Now,
                       UpdateTime = DateTime.Now,
                       Id = Guid.NewGuid(),
                       LoginName = "admin",
                       NickName = "系统管理员",
                       Salt = "3utsy5",
                       LoginPwd = "346c014f634f77c60dc6f3d5d9f76782", //123456
                       IsDelete = 0
                   });
            }
            //初始化tab菜单
            if (!await myContext.TabMenu.AsQueryable().AnyAsync())
            {
                List<TabMenuEntity> list = new List<TabMenuEntity>
                {
                    new TabMenuEntity() {
                       CreateTime = DateTime.Now,
                       Id = Guid.NewGuid(),
                       ParentGid = Guid.NewGuid(),
                       Name = "菜单一",
                       SortNum = 1,
                   },
                    new TabMenuEntity() {
                       CreateTime = DateTime.Now,
                       Id = Guid.NewGuid(),
                       ParentGid = Guid.NewGuid(),
                       Name = "菜单二",
                       SortNum = 2,
                   },
                    new TabMenuEntity() {
                       CreateTime = DateTime.Now,
                       Id = Guid.NewGuid(),
                       ParentGid = Guid.NewGuid(),
                       Name = "菜单三",
                       SortNum = 3,
                   },
                    new TabMenuEntity() {
                       CreateTime = DateTime.Now,
                       Id = Guid.NewGuid(),
                       ParentGid = Guid.NewGuid(),
                       Name = "菜单四",
                       SortNum = 4,
                   },
                };
                await myContext.TabMenu.AddRangeAsync(list);
            }

            // ... 初始化数据代码 - 全部执行
            await myContext.SaveChangesAsync();
        }
    }

    // 在使用context前调用一次
    //Database.SetInitializer<DbContext>(new MyDatabaseInitializer());
}
