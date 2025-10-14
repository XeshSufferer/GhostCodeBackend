using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class CommentsChunk
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    public string PostId { get; set; }
    public int ChunkIndex { get; set; }
    public List<Comment> Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    
}