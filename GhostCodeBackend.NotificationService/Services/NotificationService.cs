using GhostCodeBackend.NotificationService.Hubs;
using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Сache;
using Microsoft.AspNetCore.SignalR;

namespace GhostCodeBackend.NotificationService.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubCtx;
    private readonly ICacheService _cache;
    
    
    public NotificationService(IHubContext<NotificationHub> hub, ICacheService cache)
    {
        _hubCtx = hub;
        _cache = cache;
    }

    public async Task NotifyAsync(NotificationRequestDTO msg)
    {
        if (await _cache.ExistsAsync($"NotificationSessions:{msg.ReceiverId}"))
        {
            var receiverConnectionId = await _cache.GetAsync<string?>($"NotificationSessions:{msg.ReceiverId}");
            if (receiverConnectionId != null)
            {
                await _hubCtx.Clients.Client(receiverConnectionId).SendAsync("ReceiveNotification", msg.Notification);
            }
        }
    }
}