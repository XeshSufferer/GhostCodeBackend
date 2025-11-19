using Cassandra;
using Cassandra.Mapping;
using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.ChatService.Repositories;

public class MessagesRepository : IMessagesRepository
{
    private readonly IMapper _mapper;

    public MessagesRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task AddMessageAsync(Message message)
    {
        var record = MessageRecord.FromMessage(message);
        await _mapper.InsertAsync(record);
    }

    public async Task<MessageChunk> GetChunkByIndexAsync(string chatId, int index)
    {
        var total = await GetTotalChunkCountAsync(chatId);
        if (index < 1 || index > total) return new MessageChunk { ChatId = chatId, ChunkIndex = index };

        var buckets = await GetAllWeekBucketsAsync(chatId);
        var targetBucket = buckets[index - 1];

        var records = await _mapper.FetchAsync<MessageRecord>(
            "SELECT * FROM chat_messages WHERE chat_id = ? AND week_bucket = ?",
            chatId, targetBucket);

        return new MessageChunk
        {
            ChatId = chatId,
            ChunkIndex = index,
            Messages = records.Select(r => r.ToMessage()).ToList()
        };
    }

    public async Task<int> GetTotalChunkCountAsync(string chatId)
    {
        return (await GetAllWeekBucketsAsync(chatId)).Count;
    }

    private async Task<List<int>> GetAllWeekBucketsAsync(string chatId)
    {
        var results = await _mapper.FetchAsync<WeekBucketResult>(
            "SELECT DISTINCT week_bucket FROM chat_messages WHERE chat_id = ?", chatId);

        return results.Select(x => x.week_bucket)
            .OrderBy(x => x)
            .ToList();
    }
}

public class WeekBucketResult
{
    public int week_bucket { get; set; }
}