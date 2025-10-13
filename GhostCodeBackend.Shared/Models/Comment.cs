using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }
    public string AuthorId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}