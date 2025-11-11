using GhostCodeBackend.Shared.DTO.Interservice;

namespace GhostCodeBackend.NotificationService.Services;

public interface INotificationService
{
    Task NotifyAsync(NotificationRequestDTO msg);
}