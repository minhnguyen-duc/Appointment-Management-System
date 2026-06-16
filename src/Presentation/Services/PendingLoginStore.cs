namespace Presentation.Services;

/// <summary>
/// In-memory store for one-time login tickets.
/// Fixes "Headers are read-only" error in Blazor Server.
/// </summary>
public sealed class PendingLoginStore
{
    private readonly record struct Entry(List<System.Security.Claims.Claim> Claims, DateTime ExpiresAt);
    private readonly Dictionary<string, Entry> _store = new();
    private readonly Lock _lock = new();

    public string Store(List<System.Security.Claims.Claim> claims)
    {
        Purge();
        var ticket = Guid.NewGuid().ToString("N");
        lock (_lock)
            _store[ticket] = new Entry(claims, DateTime.UtcNow.AddSeconds(60));
        return ticket;
    }

    public List<System.Security.Claims.Claim>? Consume(string ticket)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(ticket, out var entry)) return null;
            _store.Remove(ticket);
            return entry.ExpiresAt > DateTime.UtcNow ? entry.Claims : null;
        }
    }

    private void Purge()
    {
        lock (_lock)
        {
            var expired = _store.Where(kv => kv.Value.ExpiresAt <= DateTime.UtcNow).Select(kv => kv.Key).ToList();
            foreach (var k in expired) _store.Remove(k);
        }
    }
}
