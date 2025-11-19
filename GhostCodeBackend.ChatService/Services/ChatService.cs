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

    public async Task<Result<List<Chat>>> GetChatsByMemberId(string memberId) => await GetChatsById__Internal(memberId);

    public async Task<Result<List<string>>> GetChatsIdsByMemberId(string memberId)
    {
        var chats = await GetChatsById__Internal(memberId);

        if (chats.IsSuccess)
        {
            var ids = chats.Value.Select(c => c.Id).ToList();
            return Result<List<string>>.Success(ids);
        }
        
        return Result<List<string>>.Failure(chats.Error);
    }
    
    private async Task<Result<List<Chat>>> GetChatsById__Internal(string memberId)
    {
        try
        {
            var chat = await _chats.GetChatsByMember(memberId);
            return chat;
        }
        catch (Exception e)
        {
            return Result<List<Chat>>.Failure(e.Message);
        }
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