using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crm.WebApp.Models
{
    /// <summary>
    /// layui树形组件
    /// </summary>
    public class LayuiTreeModel
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 子级
        /// </summary>
        public List<LayuiTreeModel> children { get; set; }

    }
}
