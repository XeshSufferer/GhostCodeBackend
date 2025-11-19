using GhostCodeBackend.ChatService.Repositories;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Services;

public class ChatService : IChatService
{
    
    private readonly IChatsRepository _chats;
    
    public ChatService(IChatsRepository chats)
    {
        _chats = chats;
    }

    public async Task<Result<Chat>> CreateChat(string aliceId, string bobId)
    {
        try
        {
            var chat = new Chat()
            {
                CreatedAt = DateTime.UtcNow,
                MembersIds = new HashSet<string>()
                {
                    aliceId,
                    bobId
                },
                Name = "yet another chat",
            };
            
            var result = await _chats.TryCreateChat(chat);

            return result.IsSuccess ? 
                Result<Chat>.Success(result.Value) : Result<Chat>.Failure(result.Error);
        }
        catch (Exception e)
        {
            return Result<Chat>.Failure(e.Message);
        }
    }
}