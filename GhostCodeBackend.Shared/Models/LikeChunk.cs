using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class LikeChunk
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    public string PostId { get; set; }
    public int ChunkIndex { get; set; }
    public string[] Users { get; set; }
}