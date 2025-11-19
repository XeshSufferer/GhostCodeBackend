using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Services;

public interface IChatService
{
    Task<Result<Chat>> CreateChat(string aliceId, string bobId);
}