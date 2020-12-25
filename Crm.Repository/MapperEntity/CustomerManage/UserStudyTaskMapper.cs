using Crm.Repository.TbEntity;
using System;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 学员学习任务
    /// </summary>
    [AutoMappers(typeof(UserStudyTaskEntity))]
    public class UserStudyTaskMapper : BaseEntityMapper
    {
        /// <summary>
        /// 学生ID
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// 课程ID
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 视频ID
        /// </summary>
        public string VideoId { get; set; }

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
