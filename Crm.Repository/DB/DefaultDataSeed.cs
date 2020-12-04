
using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            //初始化系统菜单
            if (!await myContext.SystemMenu.AsQueryable().AnyAsync())
            {
                List<SystemMenuEntity> list = Add_Default_Menu();
                await myContext.SystemMenu.AddRangeAsync(list);
            }

            //初始化系统角色标签
            if (!await myContext.UserLabel.AsQueryable().AnyAsync())
            {
                await myContext.UserLabel.AddAsync(
                   new UserLabel()
                   {
                       CreateTime = DateTime.Now,
                       Id = Guid.Parse("28B0DDEA-62C9-40F5-BBD6-7C9377D92879"),
                       ImgPath = "/images/admin.png",
                       IsDelete = 0,
                       LabelName = "管理员"
                   });
            }

            //初始化系统管理员账号
            if (!await myContext.User.AsQueryable().AnyAsync())
            {
                await myContext.User.AddAsync(
                   new User()
                   {
                       CreateTime = DateTime.Now,
                       UpdateTime = DateTime.Now,
                       Id = Guid.Parse("6669E51A-0E6C-48C4-D461-08D801EBA646"),
                       LoginName = "admin",
                       NickName = "系统管理员",
                       Salt = "3utsy5",
                       LoginPwd = "346c014f634f77c60dc6f3d5d9f76782", //123456
                       IsDelete = 0,
                       RoleId = Guid.Parse("D3CE971B-53A2-455C-A472-CE3717C65E6E"),
                       LabelId = Guid.Parse("28B0DDEA-62C9-40F5-BBD6-7C9377D92879"),
                       Sex = 1,
                       MyIntroduce = "系统管理员"
                   });
                await myContext.UserRole.AddAsync(
                   new UserRoleEntity()
                   {
                       CreateTime = DateTime.Now,
                       Id = Guid.Parse("D3CE971B-53A2-455C-A472-CE3717C65E6E"),
                       RoleDescribe = "最强",
                       RoleName = "超级管理员"
                   });

                await myContext.RoleMenu.AddAsync(
                   new RoleMenuEntity()
                   {
                       CreateTime = DateTime.Now,
                       Id = Guid.Parse("58F592D6-DBBA-4DF6-9473-F5A04CDE7B3A"),
                       MenuIds = "cc530376-24b6-4aa1-9ae1-fe2f686bdbb3,93aea9f4-0978-42d7-989c-26c33aecb973,d6369814-ec5f-4acc-bd78-7a90d8df623b,dff8bf33-e568-4a07-b9e3-c1caacfed2f8,dce3c81a-6fb3-40b3-a386-ca1f922ea1c8,ea7bc9a6-f99d-476b-8edf-c71430c43952,50f989d7-011a-44f6-838d-004563b5f67e,2de1eb63-819f-400b-a2d8-ba61b97a04e7",
                       RoleId = Guid.Parse("D3CE971B-53A2-455C-A472-CE3717C65E6E")
                   });
            }

            // ... 初始化数据代码 - 全部执行
            await myContext.SaveChangesAsync();
        }

        /// <summary>
        /// 添加默认菜单
        /// </summary>
        /// <returns></returns>
        protected static List<SystemMenuEntity> Add_Default_Menu()
        {
            List<SystemMenuEntity> list = new List<SystemMenuEntity>();
            SystemMenuEntity parentModel = null;//父级 
            string[] list_id = null; //子级ID
            string[] list_name = null; //子级名称
            string[] list_location = null; //子级路径

            #region 权限管理

            parentModel = new SystemMenuEntity()
            {
                Id = Guid.Parse("CC530376-24B6-4AA1-9AE1-FE2F686BDBB3"),
                ParentGid = Guid.Empty,
                Name = "权限管理",
                Location = "#",
                IsShow = 1,
                IsDelete = 0,
                CreateTime = DateTime.Now,
                SortNum = 1,
            };
            list.Add(parentModel);

            list_id = new string[] {
                            "93AEA9F4-0978-42D7-989C-26C33AECB973",
                            "D6369814-EC5F-4ACC-BD78-7A90D8DF623B"};
            list_name = new string[] {
                            "系统菜单",
                            "系统角色"};
            list_location = new string[] {
                            "/view/SystemRoleManage/SystemMenu/index.html",
                            "/view/SystemRoleManage/UserRole/index.html"};

            for (int i = 0; i < list_id.Length; i++)
            {
                var info = new SystemMenuEntity()
                {
                    Id = Guid.Parse(list_id[i]),
                    ParentGid = parentModel.Id,
                    Name = list_name[i],
                    Location = list_location[i],
                    IsShow = 1,
                    IsDelete = 0,
                    CreateTime = DateTime.Now,
                    SortNum = i + 1,
                };
                list.Add(info);
            }

            #endregion

            #region 系统管理
            parentModel = new SystemMenuEntity()
            {
                Id = Guid.Parse("DFF8BF33-E568-4A07-B9E3-C1CAACFED2F8"),
                ParentGid = Guid.Empty,
                Name = "系统管理",
                Location = "#",
                IsShow = 1,
                IsDelete = 0,
                CreateTime = DateTime.Now,
                SortNum = 2,
            };
            list.Add(parentModel);
            list_id = new string[] {
                            "50F989D7-011A-44F6-838D-004563B5F67E",
                            "2DE1EB63-819F-400B-A2D8-BA61B97A04E7",
                            "EA7BC9A6-F99D-476B-8EDF-C71430C43952",
                            "DCE3C81A-6FB3-40B3-A386-CA1F922EA1C8"};
            list_name = new string[] {
                            "登陆日志记录",
                            "操作日志记录",
                            "用户标签",
                            "账号管理"};
            list_location = new string[] {
                            "/view/SystemManage/loginLog.html",
                            "/view/SystemManage/operationLog.html",
                            "/view/SystemManage/userLabel.html",
                            "/view/SystemManage/systemUser.html"};

            for (int i = 0; i < list_id.Length; i++)
            {
                var info = new SystemMenuEntity()
                {
                    Id = Guid.Parse(list_id[i]),
                    ParentGid = parentModel.Id,
                    Name = list_name[i],
                    Location = list_location[i],
                    IsShow = 1,
                    IsDelete = 0,
                    CreateTime = DateTime.Now,
                    SortNum = i + 1,
                };
                list.Add(info);
            }
            #endregion

            return list;
        }

    }

    // 在使用context前调用一次
    //Database.SetInitializer<DbContext>(new MyDatabaseInitializer());
}
