using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class LikeChunk
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string PostId { get; set; }
    public int SegmentId { get; set; }
    public string[] Users { get; set; }
}