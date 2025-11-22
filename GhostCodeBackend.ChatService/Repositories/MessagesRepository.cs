using Cassandra;
using Cassandra.Mapping;
using GhostCodeBackend.Shared.Models;
using Microsoft.Extensions.Logging;

namespace GhostCodeBackend.ChatService.Repositories;

public interface IMessagesRepository
{
    Task<Result<Message>> TryAddMessageAsync(Message message);
    Task<Result<MessageChunk>> TryGetChunkByIndexAsync(string chatId, int chunkIndex, int limit = 50);
    Task<Result<int>> TryGetTotalChunkCountAsync(string chatId);
}

public class MessagesRepository : IMessagesRepository
{
    private readonly IMapper _scyllaMapper;
    private readonly IMongoCollection<Chat> _chatsCollection;
    private readonly ILogger<MessagesRepository> _logger;

    public MessagesRepository(
        IMapper scyllaMapper,
        IMongoCollection<Chat> chatsCollection,
        ILogger<MessagesRepository> logger)
    {
        _scyllaMapper = scyllaMapper;
        _chatsCollection = chatsCollection;
        _logger = logger;
    }

    public async Task<Result<Message>> TryAddMessageAsync(Message message)
    {
        if (message == null)
            return Result<Message>.Failure("Message is null");

        try
        {
           
            var record = MessageRecord.FromMessage(message);
            await _scyllaMapper.InsertAsync(record);

           
            await UpdateChatMetadataAsync(message.ChatId, record.WeekBucket, message.SenderId);

            return Result<Message>.Success(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add message to chat {ChatId}", message.ChatId);
            return Result<Message>.Failure("Failed to save message");
        }
    }

    public async Task<Result<MessageChunk>> TryGetChunkByIndexAsync(string chatId, int chunkIndex, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            return Result<MessageChunk>.Failure("Invalid chat ID");
        if (chunkIndex < 1)
            return Result<MessageChunk>.Failure("Chunk index must be >= 1");
        if (limit <= 0 || limit > 100)
            return Result<MessageChunk>.Failure("Invalid limit");

        try
        {
            var weekBuckets = await GetAllWeekBucketsAsync(chatId);
            if (chunkIndex > weekBuckets.Count)
                return Result<MessageChunk>.Failure("Chunk index out of range");

            var targetWeek = weekBuckets[chunkIndex - 1];
            var query = @"
                SELECT * FROM chat_messages 
                WHERE chat_id = ? AND week_bucket = ? 
                ORDER BY id DESC 
                LIMIT ?";

            var records = await _scyllaMapper.FetchAsync<MessageRecord>(query, chatId, targetWeek, limit);
            var messages = records.Select(r => r.ToMessage()).ToList();

            var chunk = new MessageChunk
            {
                ChatId = chatId,
                ChunkIndex = chunkIndex,
                Messages = messages,
                HasMore = messages.Count == limit
            };

            return Result<MessageChunk>.Success(chunk);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get chunk {ChunkIndex} for chat {ChatId}", chunkIndex, chatId);
            return Result<MessageChunk>.Failure("Failed to load messages");
        }
    }

    public async Task<Result<int>> TryGetTotalChunkCountAsync(string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            return Result<int>.Failure("Invalid chat ID");

        try
        {
            var count = (await GetAllWeekBucketsAsync(chatId)).Count;
            return Result<int>.Success(count);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get chunk count for chat {ChatId}", chatId);
            return Result<int>.Failure("Failed to load chunk count");
        }
    }

    private async Task<List<int>> GetAllWeekBucketsAsync(string chatId)
    {
        var query = "SELECT DISTINCT week_bucket FROM chat_messages WHERE chat_id = ?";
        var results = await _scyllaMapper.FetchAsync<WeekBucketResult>(query, chatId);
        return results.Select(x => x.week_bucket).OrderBy(x => x).ToList();
    }

    private async Task UpdateChatMetadataAsync(string chatId, int weekBucket, string senderId)
    {
        var filter = Builders<Chat>.Filter.Eq(x => x.Id, chatId);
        var chat = await _chatsCollection.Find(filter).FirstOrDefaultAsync();

        if (chat == null)
        {
            
            var newChat = new Chat
            {
                Id = chatId,
                MembersIds = new HashSet<string> { senderId },
                Name = "Direct Chat",
                CreatedAt = DateTime.UtcNow,
                MessagesCount = 1,
                MessagesChunkCount = 1
            };
            await _chatsCollection.InsertOneAsync(newChat);
        }
        else
        {
           
            var update = Builders<Chat>.Update
                .Inc(x => x.MessagesCount, 1);

            
            var currentChunks = chat.MessagesChunkCount;
            var actualChunks = (await GetAllWeekBucketsAsync(chatId)).Count;

            if (actualChunks > currentChunks)
            {
                update = update.Set(x => x.MessagesChunkCount, actualChunks);
            }

        
            if (!chat.MembersIds.Contains(senderId))
            {
                update = update.AddToSet(x => x.MembersIds, senderId);
            }

            await _chatsCollection.UpdateOneAsync(filter, update);
        }
    }
}

public class WeekBucketResult
{
    public int week_bucket { get; set; }
}

public class MessageChunk
{
    public string ChatId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public List<Message> Messages { get; set; } = new();
    public bool HasMore { get; set; }
}