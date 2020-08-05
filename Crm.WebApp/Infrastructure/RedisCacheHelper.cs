
using Currency.Common.Redis;
using Microsoft.Extensions.Configuration;

namespace CMS.Project.Infrastructure
{
    public class RedisCacheHelper
    {
        private IStaticRedisManager cacheManager;

        public RedisCacheHelper(IConfiguration configuration)
        {
            string conn = configuration.GetValue<string>("RedisConfig:RedisStack");
            cacheManager = new RedisCacheManager(new RedisConnectionWrapper(conn), conn);
        }

        /// <summary>
        /// 获取全部函数
        /// </summary>
        public IStaticRedisManager All
        {
            get
            {
                return cacheManager;
            }
        }

        /// <summary>
        /// 根据 KEY 获取结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return cacheManager.Get<T>(key);
        }

        /// <summary>
        /// 插入一条记录缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheTime"></param>
        public void Set(string key, object value, int cacheTime)
        {
            cacheManager.Set(key, value, cacheTime);
        }

        /// <summary>
        /// 插入一条记录缓存
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            cacheManager.Remove(key);
        }
    }
}
