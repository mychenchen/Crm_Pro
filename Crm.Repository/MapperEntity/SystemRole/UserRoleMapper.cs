using Crm.Repository.TbEntity;

namespace Crm.Repository.MapperEntity
{
    /// <summary>
    /// 用户角色
    /// </summary>
    [AutoMappers(typeof(UserRoleEntity))]
    public class UserRoleMapper : BaseEntityMapper
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 角色描述 
        /// </summary>
        public string RoleDescribe { get; set; }

    }
}
