using DeviceDetectorNET.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smart_Medical.Pharmacy;
using Smart_Medical.Until;
using Smart_Medical.Until.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Smart_Medical.Redis
{
    // Scoped 或 Transient 都可以，取决于您的业务逻辑。
    // 如果在同一个请求内，该服务需要管理某个特定状态，则使用 Scoped。
    // 如果是无状态的辅助操作，Transient 也可以。
    [Dependency(ServiceLifetime.Scoped)]
    public class RedisHelper<T> : IRedisHelper<T>, ITransientDependency where  T:class // 实现泛型接口，并标记为瞬时依赖
    {
        private readonly IDistributedCache<T> cache;
        private readonly ILogger<RedisHelper<T>> logger;

        public RedisHelper(IDistributedCache<T> cache, ILogger<RedisHelper<T>> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }
       
        /// <summary>
        /// 判断键是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string key)
        {
            // IDistributedCache 没有直接的 Exists 方法，
            // 我们可以通过尝试获取来判断是否存在（但不精确，因为可能返回默认值）
            // 或者使用 IDistributedCacheEntrySerializer 来检查底层字节
            // 更可靠的做法是直接访问 StackExchange.Redis 的低级 API，但这会打破抽象。
            // 对于简单的存在性检查，GetAsync并判断是否为null通常足够。
            // 但对于值类型T，GetAsync可能返回T的默认值，而不是null。
            // 更好的方法是使用TryGetAsync，但IDistributedCache没有直接提供。
            // 如果您的T是引用类型，则此方法是可靠的。
            var value = await cache.GetAsync(key);
            return value != null;
        }

        public async Task<T> GetAsync(string key, Func<Task<T>> func)
        {
            // 1. 先查缓存
            var value = await cache.GetAsync(key);
            if (value != null)
            {
                logger.LogInformation($"[RedisHelper] 缓存命中 key: {key}");
                return value;
            }

            logger.LogInformation($"[RedisHelper] 缓存无命中 key: {key}");

            // 2. 缓存无命中，调用委托获取数据
            var result = await func();
            if (result != null)
            {
                // 3. 写入缓存
                await cache.SetAsync(key, result);
            }
            return result;
        }

        public async Task RemoveAsync(string key)
        {
            await cache.RemoveAsync(key);
            logger.LogInformation($"[RedisHelper] Cache removed for key: {key}");
        }

        public async Task SetAsync(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (absoluteExpirationRelativeToNow.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow.Value;
            }
            if (slidingExpiration.HasValue)
            {
                options.SlidingExpiration = slidingExpiration.Value;
            }

            // 如果都没有设置，可以给一个默认值
            if (!absoluteExpirationRelativeToNow.HasValue && !slidingExpiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // 默认 30 分钟
            }

            await cache.SetAsync(key, value, options);
        }

     
    }
}
