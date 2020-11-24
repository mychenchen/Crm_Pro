using System.Collections.Generic;

namespace Crm.WebApp.Models
{
    /// <summary>
    /// 系统右侧菜单模型
    /// </summary>
    public class SystemLeftMenuModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string icon { get; set; }

        /// <summary>
        /// 跳转链接
        /// </summary>
        public string link { get; set; }

        /// <summary>
        /// 子级
        /// </summary>
        public List<SystemLeftMenuModel> children { get; set; }

    }
}
