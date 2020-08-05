using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CMS.Project.Infrastructure
{
    /// <summary>
    /// MongoDB 服务
    /// </summary>
    public class MongoDBService
    {
        private IMongoDatabase db;

        public MongoDBService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("MongoConfig:MongoDatabaseConnection"));
            db = client.GetDatabase(configuration.GetValue<string>("MongoConfig:MongoDatabase"));
        }

        /// <summary>
        /// 返回集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tabName"></param>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>(string tabName = "tbBusLocationLogModel")
        {
            return db.GetCollection<T>(tabName);
        }

    }
}
