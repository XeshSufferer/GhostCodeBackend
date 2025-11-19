using GhostCodeBackend.ChatService.Repositories;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Services;

public class MessagingService : IMessagingService
{

    private readonly IMessagesRepository _messages;
    
    public MessagingService(IMessagesRepository messages)
    {
        _messages = messages;
    }
    
    public async Task<Result> AddMessageToChat(string senderId, string chatId, string replyToId, string message)
    {
        try
        {
            
            var msg = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Text = message,
                CreatedAt = DateTime.UtcNow,
                PictureLinks = new List<string>(),
                Reactions = new List<Reaction>(),
                ReplyToId = replyToId
            };

            await _messages.AddMessageAsync(msg);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
}