using System.Collections.Concurrent;
using GhostCodeBackend.Shared.Сache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GhostCodeBackend.NotificationService.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICacheService _cache;

    public NotificationHub(ICacheService cache)
    {
        _cache = cache;
    }
    
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity?.Name != null)
            await _cache.SetAsync($"NotificationSessions:{Context.User.Identity.Name}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if(Context.User?.Identity?.Name != null && await _cache.ExistsAsync($"NotificationSessions:{Context.User.Identity.Name}"))
            await _cache.RemoveAsync($"NotificationSessions:{Context.User.Identity.Name}");
        await base.OnDisconnectedAsync(exception);
    }
}