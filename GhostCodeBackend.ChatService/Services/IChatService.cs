using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Services;

public interface IChatService
{
    Task<Result<Chat>> CreateChat(string aliceId, string bobId);
    Task<Result<List<Chat>>> GetChatsByMemberId(string memberId);
    Task<Result<List<string>>> GetChatsIdsByMemberId(string memberId);
}