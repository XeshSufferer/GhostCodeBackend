using GhostCodeBackend.ChatService.Services;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Сache;
using Microsoft.AspNetCore.SignalR;

namespace GhostCodeBackend.ChatService.Hubs;

public class ChatHub : Hub
{

    private readonly ICacheService _cache;
    private readonly IMessagingService _messaging;
    private readonly IChatService _chats;
    

    public ChatHub(ICacheService cache, IMessagingService messaging, IChatService chats)
    {
        _chats = chats;
        _cache = cache;
        _messaging = messaging;
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity?.Name != null)
            await _cache.SetAsync($"ChatSessions:{Context.User.Identity.Name}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if(Context.User?.Identity?.Name != null && await _cache.ExistsAsync($"ChatSessions:{Context.User.Identity.Name}"))
            await _cache.RemoveAsync($"ChatSessions:{Context.User.Identity.Name}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task CreateChat(CreateChatRequestDTO request)
    {
        var chat = await _chats.CreateChat(Context.User.Identity.Name, request.AliceId);
        if (chat.IsSuccess)
        {
            await Clients.Caller.SendAsync("ChatCreatedSuccessfully", chat.Value);
            var aliceConnectionId = await _cache.GetAsync<string?>($"ChatSessions:{request.AliceId}");
            if(aliceConnectionId != null)
                await Clients.Client(aliceConnectionId).SendAsync("ChatCreatedSuccessfully", chat.Value);
            return;
        }
        await Clients.Caller.SendAsync("ChatCreationFailed", chat.Error);
    }

    public async Task SendMessage(SendMessageRequestDTO request)
    {
        var getChatTask = _chats.GetChat(request.ChatId);
        var addMessageTask = _messaging.AddMessageToChat(Context.User.Identity.Name, 
            request.ChatId, request.ReplyTo, request.Message);

        var msg = await addMessageTask;
        if (!msg.IsSuccess)
        {
            await Clients.Caller.SendAsync("SendMessageError", msg.Error);
            return;
        }

        
        var chat = await getChatTask;
        if (!chat.IsSuccess)
        {
            await Clients.Caller.SendAsync("SendMessageError", chat.Error);
            return;
        }
        
        foreach (string memberId in chat.Value.MembersIds)
        {
            var connectionId = await _cache.GetAsync<string>($"ChatSessions:{memberId}");
            if (connectionId != null)
                await Clients.Client(connectionId).SendAsync("MessageReceive", msg.Value);
        }
    }
    
    
}