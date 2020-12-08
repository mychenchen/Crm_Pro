using System.Collections.Generic;

namespace Crm.Service.BaseHelper
{
    /// <summary>
    /// 分页查询返回
    /// </summary>
    public class ServicePageResult<T>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int TotalSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPage
        {
            get
            {
                int num = TotalSize / PageSize;
                if (TotalSize > 0 && (TotalSize % PageSize) > 0)
                {
                    num = num + 1;
                }
                return num;
            }
        }

        public List<T> List { get; set; }
    }
}
