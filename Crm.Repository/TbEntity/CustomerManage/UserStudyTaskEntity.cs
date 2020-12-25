using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Repository.TbEntity
{
    /// <summary>
    /// 学员学习任务
    /// </summary>
    [Table("tb_StudyTask")]
    public class UserStudyTaskEntity : BaseEntity
    {
        /// <summary>
        /// 学生ID
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// 课程ID
        /// </summary>
        [Required]
        public Guid CourseId { get; set; }

        /// <summary>
        /// 视频ID
        /// </summary>
        [Required]
        public Guid VideoId { get; set; }

        /// <summary>
        /// 视频名称
        /// </summary>
        public string VideoName { get; set; }

        /// <summary>
        /// 学习时长
        /// </summary>
        public int TimeLong { get; set; }

        /// <summary>
        /// 是否学习 0未学习 1已学习
        /// </summary>
        public int IsStudy { get; set; }

    }
}
