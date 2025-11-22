using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Repositories;

public interface IMessagesRepository
{
    Task<Result<Message>> TryAddMessageAsync(Message message);
    Task<Result<MessageChunk>> TryGetChunkByIndexAsync(string chatId, int chunkIndex, int limit = 50);
    Task<Result<int>> TryGetTotalChunkCountAsync(string chatId);
}