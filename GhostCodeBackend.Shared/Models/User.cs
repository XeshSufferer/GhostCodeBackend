using GhostCodeBackend.Shared.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public string RecoveryCodeHash { get; set; }
    public Role Role { get; set; } = Role.User;
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.None;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime SubscribeExpiresAt { get; set; }
    
    // Customize
    public string Description { get; set; } = "It's me!";
    public string AvatarLink { get; set; } = "default";
    public string HeaderLink { get; set; } = "default";
    
    
}
