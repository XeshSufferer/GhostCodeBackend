using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Repositories;

public interface IChatsRepository
{
    Task<Result<Chat>> TryCreateChat(Chat chat);
    Task<Result> TryDeleteChat(string chatId);
    Task<Result<Chat>> TryGetChat(string chatId);
    Task<Result<Chat>> TryUpdateChat(string chatId, Chat chat);
    Task<Result<List<Chat>>> GetChatsByMember(string memberId);
}