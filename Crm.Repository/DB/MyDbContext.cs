using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Repository.DB
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        #region 系统

        /// <summary>
        /// 系统用户表
        /// </summary>
        public DbSet<User> User { get; set; }

        /// <summary>
        /// 用户标签
        /// </summary>
        public DbSet<UserLabel> UserLabel { get; set; }

        /// <summary>
        /// 登录记录
        /// </summary>
        public DbSet<UserLoginLog> UserLoginLog { get; set; }

        /// <summary>
        /// 用户操作日志
        /// </summary>
        public DbSet<OperationLogEntity> OperationLog { get; set; }

        #endregion

        #region 菜单权限

        /// <summary>
        /// 系统菜单
        /// </summary>
        public DbSet<SystemMenuEntity> SystemMenu { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public DbSet<UserRoleEntity> UserRole { get; set; }

        /// <summary>
        /// 角色菜单
        /// </summary>
        public DbSet<RoleMenuEntity> RoleMenu { get; set; }

        #endregion

        #region 导航公告

        /// <summary>
        /// 新闻
        /// </summary>
        public DbSet<HotNewsEntity> HotNews { get; set; }

        /// <summary>
        /// 热点轮播
        /// </summary>
        public DbSet<HotSpotEntity> HotSpot { get; set; }

        /// <summary>
        /// 公告公示
        /// </summary>
        public DbSet<NoticeEntity> Notice { get; set; }

        /// <summary>
        /// tab菜单
        /// </summary>
        public DbSet<TabMenuEntity> TabMenu { get; set; }

        #endregion

        #region 产品

        /// <summary>
        /// 产品信息
        /// </summary>
        public DbSet<ProductEntity> Product { get; set; }

        /// <summary>
        /// 产品视频详情
        /// </summary>
        public DbSet<ProductVideoEntity> ProductVideo { get; set; }

        #endregion

        #region 客户

        /// <summary>
        /// 学员表
        /// </summary>
        public DbSet<UserStudentEntity> UserStudent { get; set; }

        /// <summary>
        /// 用户评论(学员-老师)
        /// </summary>
        public DbSet<UserCommentEntity> UserComment { get; set; }

        /// <summary>
        /// 用户订单(学员)
        /// </summary>
        public DbSet<UserOrderEntity> UserOrder { get; set; }

        #endregion

    }
}
