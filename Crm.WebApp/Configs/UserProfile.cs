using AutoMapper;
using Crm.Repository.MapperEntity;
using Crm.Repository.TbEntity;

namespace Crm.WebApp.Models.Configs
{
    public class UserProfile : Profile
    {

        public UserProfile()
        {
            // 添加尽可能多的这些行的，因为你需要映射你的对象

            CreateMap<User, UserMapper>();
            CreateMap<UserLoginLog, UserLoginLogMapper>();
            CreateMap<HotNewsEntity, HotNewsMapper>();
            CreateMap<HotSpotEntity, HotSpotMapper>();
            CreateMap<NoticeEntity, NoticeMapper>();
            CreateMap<TabMenuEntity, TabMenuMapper>();
            CreateMap<OperationLogEntity, OperationLogMapper>();

            CreateMap<ProductEntity, ProductMapper>();
            CreateMap<ProductVideoEntity, ProductVideoMapper>();
        }
    }
}
