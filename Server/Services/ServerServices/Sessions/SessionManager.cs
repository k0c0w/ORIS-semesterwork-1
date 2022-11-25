using Microsoft.Extensions.Caching.Memory;

namespace Server.Services.ServerServices;

public class SessionManager
{
    private static readonly Lazy<SessionManager> lazy = new Lazy<SessionManager>(() => new SessionManager());

    public static SessionManager Instance => lazy.Value;

    private SessionManager() {}

    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly object _lock = new object();

    public bool CheckSession(object key)
    {
        return _cache.TryGetValue(key, out _);
    }

    private void CreateSession(object key, Func<Session> createSession, MemoryCacheEntryOptions options)
    {
        if (!_cache.TryGetValue(key, out var cacheEntry))
        {
            lock(_lock)
            {
                if (!_cache.TryGetValue(key, out cacheEntry))
                    _cache.Set(key, createSession(), options);
            }
        }
    }

    public void CreateQuickSession(object key, Func<Session> createSession)
    {
        CreateSession(key, createSession, 
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2)));
    }

    public void CreateLongSession(object key, Func<Session> createSession)
    {
        CreateSession(key, createSession, 
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
    }
    
    public bool TryGetSession(object key, out Session? session)
    {
        return _cache.TryGetValue(key, out session);
    }

    public void TerminateSession(object key) => _cache.Remove(key);
}    