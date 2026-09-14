using Microsoft.Extensions.Caching.Memory;

namespace MassTransitDemo.ApiService.Helpers;

public class DebounceGuard(IMemoryCache cache)
{
    public bool TryAcquire(string key, TimeSpan debounceInterval)
    {
        if (cache.TryGetValue(key, out _))
        {
            return false;
        }

        cache.Set(key, true, debounceInterval);
        return true;
    }
}