using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Services;

public interface IMessagingService
{
    Task<Result<Message>> AddMessageToChat(string senderId, string chatId, string replyToId, string message);

}