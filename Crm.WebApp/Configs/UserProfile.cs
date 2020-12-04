using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;

namespace Crm.WebApp.Models.Configs
{
    /// <summary>
    /// 数据表映射
    /// </summary>
    public class UserProfile : Profile
    {

        public UserProfile()
        {
            // 添加尽可能多的这些行的，因为你需要映射你的对象

            CreateMap<User, UserMapper>();
            CreateMap<UserLoginLog, UserLoginLogMapper>();

            CreateMap<TabMenuEntity, TabMenuMapper>();
            CreateMap<OperationLogEntity, OperationLogMapper>();

            CreateMap<HotNewsEntity, HotNewsMapper>();
            CreateMap<HotSpotEntity, HotSpotMapper>();
            CreateMap<NoticeEntity, NoticeMapper>();

            CreateMap<ProductTypeEntity, ProductTypeMapper>();
            CreateMap<ProductEntity, ProductMapper>();
            CreateMap<ProductVideoEntity, ProductVideoMapper>();


            CreateMap<SystemMenuEntity, SystemMenuMapper>();
            CreateMap<UserRoleEntity, UserRoleMapper>();
            CreateMap<RoleMenuEntity, RoleMenuMapper>();

            CreateMap<UserStudentEntity, UserStudentMapper>();
            CreateMap<UserCommentEntity, UserCommentMapper>();
            CreateMap<UserOrderEntity, UserOrderMapper>();

        }
    }
}
