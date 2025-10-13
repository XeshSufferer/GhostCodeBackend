using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class CommentsChunk
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string PostId { get; set; }
    public List<Comment> Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    
}