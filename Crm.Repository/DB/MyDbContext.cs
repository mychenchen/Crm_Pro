using Crm.Repository.TbEntity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Repository.DB
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        #region 映射到数据库

        /// <summary>
        /// 系统用户表
        /// </summary>
        public DbSet<User> User { get; set; }

        /// <summary>
        /// 登录记录
        /// </summary>
        public DbSet<UserLoginLog> UserLoginLog { get; set; }

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

        /// <summary>
        /// 用户操作日志
        /// </summary>
        public DbSet<OperationLogEntity> OperationLog { get; set; }

        /// <summary>
        /// 产品信息
        /// </summary>
        public DbSet<ProductEntity> Product { get; set; }

        /// <summary>
        /// 产品视频详情
        /// </summary>
        public DbSet<ProductVideoEntity> ProductVideo { get; set; }

        #endregion
    }
}
