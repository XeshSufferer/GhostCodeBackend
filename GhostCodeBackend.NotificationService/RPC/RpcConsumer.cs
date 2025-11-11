using GhostCodeBackend.NotificationService.Hubs;
using GhostCodeBackend.NotificationService.Services;
using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.Сache;
using Microsoft.AspNetCore.SignalR;

namespace GhostCodeBackend.NotificationService.RPC;

public class RpcConsumer : IRpcConsumer
{

    private readonly IRabbitMQService _rabbit;
    private readonly INotificationService _notificationService;

    public RpcConsumer(IRabbitMQService rabbit, INotificationService notificationService)
    {
        _rabbit = rabbit;
        _notificationService = notificationService;
    }

    public async Task InitConsume()
    {
        await _rabbit.StartConsumingAsync<NotificationRequestDTO>("Notifications.SendNotification", async msg =>
        {
            await _notificationService.NotifyAsync(msg);
        });
    }
}