using Crm.Repository.DB;
using Crm.Repository.TbEntity;
using Crm.Service.BaseHelper;

namespace Crm.Service.CustomerService
{
    /// <summary>
    /// 学员管理
    /// </summary>
    public class UserStudyTaskService : BaseServiceRepository<UserStudyTaskEntity>, IUserStudyTaskService
    {
        public UserStudyTaskService(MyDbContext mydb) : base(mydb)
        {
        }


    }
}
