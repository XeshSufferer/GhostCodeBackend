using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.Shared.DTO.Interservice;

public class NotificationRequestDTO
{
    public string ReceiverId { get; set; }
    public Notification Notification { get; set; }
}