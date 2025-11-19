using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class MessageChunk
{
    public string ChatId { get; set; }
    public int ChunkIndex { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
}