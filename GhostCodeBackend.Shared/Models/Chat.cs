using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }
    public string Name { get; set; }
    public HashSet<string> MembersIds { get; set; } = new();
    public LinkedList<Message> CachedLastMessages { get; set; } = new();
    public int MessagesChunkCount { get; set; }
    public int MessagesCount { get; set; }
    public int MessagesChunkSize { get; set; }
    public int MessagesCacheSize { get; set; }
    [BsonIgnore]
    public int MessagesChunkingThreshold
    {
        get => MessagesCacheSize + MessagesChunkSize;
    }
    public DateTime CreatedAt { get; set; }
    
    public void AddMessageToCache(Message message)
    {
        CachedLastMessages.AddFirst(message);
        MessagesCacheSize += 1;
    }
}