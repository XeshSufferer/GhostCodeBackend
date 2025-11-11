using GhostCodeBackend.Shared.Models.Enums;

namespace GhostCodeBackend.Shared.Models;

public class Notification
{
    public string Title { get; set; } = "FooNotification";
    public string Message { get; set; } = "Hello ByXesh!";
    public NotificationTypes Type { get; set; } = NotificationTypes.Success;
}