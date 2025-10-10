using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class User
{
    [BsonId]
    public string Id { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public string RecoveryCodeHash { get; set; }
}