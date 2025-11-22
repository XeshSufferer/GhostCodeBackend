using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.ChatService.Repositories;

public class ChatsRepository : IChatsRepository
{
    private readonly IMongoCollection<Chat> _chats;
    private readonly ILogger<ChatsRepository> _logger;

    public ChatsRepository(IMongoDatabase db, ILogger<ChatsRepository> logger)
    {
        _chats = db.GetCollection<Chat>("chats");
        _logger = logger;
        // Создаём индекс один раз (можно вынести в инициализацию приложения)
        EnsureIndexes();
    }

    private async void EnsureIndexes()
    {
        try
        {
            var indexKeys = Builders<Chat>.IndexKeys.Ascending(c => c.MembersIds);
            await _chats.Indexes.CreateOneAsync(new CreateIndexModel<Chat>(indexKeys));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create index on MembersIds");
        }
    }

    public async Task<Result<Chat>> TryCreateChat(Chat chat)
    {
        if (chat == null)
            return Result<Chat>.Failure("Chat is null");

        try
        {
            await _chats.InsertOneAsync(chat);
            return Result<Chat>.Success(chat);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create chat {ChatId}", chat.Id);
            return Result<Chat>.Failure("Database error");
        }
    }

    public async Task<Result> TryDeleteChat(string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            return Result.Failure("Invalid chat ID");

        try
        {
            var result = await _chats.DeleteOneAsync(c => c.Id == chatId);
            return result.DeletedCount > 0 ? Result.Success() : Result.Failure("Chat not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete chat {ChatId}", chatId);
            return Result.Failure("Database error");
        }
    }

    public async Task<Result<Chat>> TryGetChat(string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            return Result<Chat>.Failure("Invalid chat ID");

        try
        {
            var chat = await _chats.Find(c => c.Id == chatId).FirstOrDefaultAsync();
            return chat != null 
                ? Result<Chat>.Success(chat) 
                : Result<Chat>.Failure("Chat not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get chat {ChatId}", chatId);
            return Result<Chat>.Failure("Database error");
        }
    }

    public async Task<Result<Chat>> TryUpdateChat(string chatId, Chat chat)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            return Result<Chat>.Failure("Invalid chat ID");
        if (chat == null)
            return Result<Chat>.Failure("Chat is null");
        if (chat.Id != chatId)
            return Result<Chat>.Failure("Chat ID mismatch");

        try
        {
            var result = await _chats.ReplaceOneAsync(c => c.Id == chatId, chat);
            return result.ModifiedCount > 0 
                ? Result<Chat>.Success(chat) 
                : Result<Chat>.Failure("Chat not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update chat {ChatId}", chatId);
            return Result<Chat>.Failure("Database error");
        }
    }

    public async Task<Result<List<Chat>>> GetChatsByMember(string memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
            return Result<List<Chat>>.Failure("Invalid member ID");

        try
        {
            var chats = await _chats
                .Find(c => c.MembersIds.Contains(memberId))
                .ToListAsync();

            return Result<List<Chat>>.Success(chats);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get chats for member {MemberId}", memberId);
            return Result<List<Chat>>.Failure("Database error");
        }
    }
}