using Smart_Medical.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Until.Redis
{
    public interface IRedisHelper<T>
    {
        Task SetAsync(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null);
        Task<T> GetAsync(string key, Func<Task<T>> func);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key); // 检查键是否存在
        
    }
}
