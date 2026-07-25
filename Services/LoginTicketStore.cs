using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

/// <summary>
/// One-time login tickets so the browser can hit /account/establish and receive an auth cookie.
/// Blazor Interactive Server cannot set cookies after the circuit starts.
/// </summary>
public sealed class LoginTicketStore
{
    private readonly IMemoryCache _cache;

    public LoginTicketStore(IMemoryCache cache) => _cache = cache;

    public string Create(LoggedInUserDto user)
    {
        var ticket = Guid.NewGuid().ToString("N");
        _cache.Set(CacheKey(ticket), user, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        });
        return ticket;
    }

    public bool TryTake(string ticket, out LoggedInUserDto? user)
    {
        user = null;
        if (string.IsNullOrWhiteSpace(ticket)) return false;
        var key = CacheKey(ticket);
        if (!_cache.TryGetValue(key, out LoggedInUserDto? stored) || stored == null)
            return false;
        _cache.Remove(key);
        user = stored;
        return true;
    }

    private static string CacheKey(string ticket) => $"ITBudget:LoginTicket:{ticket}";
}
