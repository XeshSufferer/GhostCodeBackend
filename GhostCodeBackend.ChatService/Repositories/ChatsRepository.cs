using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GhostCodeBackend.ChatService.Repositories;

public class ChatsRepository : IChatsRepository
{

    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<Chat> _chats;
    private readonly ILogger<ChatsRepository> _logger;
    
    public ChatsRepository(IMongoDatabase db, ILogger<ChatsRepository> logger)
    {
        _db = db;
        _chats = db.GetCollection<Chat>("chats");
        _logger = logger;
    }

    public async Task<Result<Chat>> TryCreateChat(Chat chat)
    {
        try
        {
            await _chats.InsertOneAsync(chat);
            return Result<Chat>.Success(chat);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Chat>.Failure(e.Message);
        }
    }

    public async Task<Result> TryDeleteChat(string chatId)
    {
        try
        {
            await _chats.DeleteOneAsync(c => c.Id == chatId);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result<Chat>> TryGetChat(string chatId)
    {
        try
        {
            var chat = await _chats.AsQueryable().Where(c => c.Id == chatId).FirstOrDefaultAsync();
            return Result<Chat>.Success(chat);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Chat>.Failure(e.Message);
        }
    }

    public async Task<Result<Chat>> TryUpdateChat(string chatId, Chat chat)
    {
        try
        {
            var result = await _chats.ReplaceOneAsync(c => c.Id == chatId, chat);
            return result.ModifiedCount > 0 ? Result<Chat>.Success(chat) : Result<Chat>.Failure("Chat not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<Chat>.Failure(e.Message);
        }
    }
}