using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{

    /// <summary>
    /// 用户标签
    /// </summary>
    [Table("tb_UserLable")]
    public class UserLabel : BaseEntity
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        [Required]
        [StringLength(20)]
        public string LabelName { get; set; }

        /// <summary>
        /// 标签图标
        /// </summary>
        [StringLength(200)]
        public string ImgPath { get; set; }

    }
}
