using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 映射类通用字段
    /// </summary>
    public class BaseEntityMapper
    {
        /// <summary>
        /// 数据库唯一ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建时间格式化
        /// </summary>        
        public string CreateTimeStr
        {
            get
            {
                return CreateTime == null ? "" : CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");

            }
        }

        /// <summary>
        /// 是否删除 0 正常 1 已删除
        /// </summary>
        public int IsDelete { get; set; }

        /// <summary>
        /// 创建时间格式化
        /// </summary>        
        public string IsDeleteStr
        {
            get
            {
                string str = "正常";
                if (this.IsDelete == 1)
                {
                    str = "已删除";
                }
                return str;
            }
        }
    }
}
