using AutoMapper;
using Crm.Repository;
using Currency.Common.SystemRegister;
using System.Reflection;

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

            var assembly = RuntimeHelper.GetAssemblyByName("Crm.Repository");

            var types = assembly.GetExportedTypes();
            foreach (var type in types)
            {
                if (!type.IsDefined(typeof(AutoMappersAttribute))) continue;
                var autoMapper = type.GetCustomAttribute<AutoMappersAttribute>();

                foreach (var source in autoMapper.ToSource)
                {
                    CreateMap(type, source).ReverseMap();
                }
            }

            //CreateMap<User, UserMapper>();
            //CreateMap<UserLoginLog, UserLoginLogMapper>();

            //CreateMap<TabMenuEntity, TabMenuMapper>();
            //CreateMap<OperationLogEntity, OperationLogMapper>();

            //CreateMap<HotNewsEntity, HotNewsMapper>();
            //CreateMap<HotSpotEntity, HotSpotMapper>();
            //CreateMap<NoticeEntity, NoticeMapper>();

            //CreateMap<ProductTypeEntity, ProductTypeMapper>();
            //CreateMap<ProductEntity, ProductMapper>();
            //CreateMap<ProductVideoEntity, ProductVideoMapper>();


            //CreateMap<SystemMenuEntity, SystemMenuMapper>();
            //CreateMap<UserRoleEntity, UserRoleMapper>();
            //CreateMap<RoleMenuEntity, RoleMenuMapper>();

            //CreateMap<UserStudentEntity, UserStudentMapper>();
            //CreateMap<UserCommentEntity, UserCommentMapper>();
            //CreateMap<UserOrderEntity, UserOrderMapper>();

        }

    }
}
