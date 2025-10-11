using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class RefreshToken
{
    [BsonId]
    public string Id { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}