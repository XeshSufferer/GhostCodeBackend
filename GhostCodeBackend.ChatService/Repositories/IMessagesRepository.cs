using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.ChatService.Repositories;

public interface IMessagesRepository
{
    Task<int> GetTotalChunkCountAsync(string chatId);
    Task AddMessageAsync(Message message);
    Task<MessageChunk> GetChunkByIndexAsync(string chatId, int index);
}