using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Models.Enums;

namespace GhostCodeBackend.Shared.DTO.Interservice;

public class DataForJWTWrite
{
    public string Id { get; set; }
    public Role Role { get; set; }
    public SubscriptionTier Tier { get; set; }
    public DateTime SubscribeExpiresAt { get; set; }

    public DataForJWTWrite MapFromDomainUser(User user)
    {
        Id = user.Id;
        Role = user.Role;
        Tier = user.Tier;
        SubscribeExpiresAt = user.SubscribeExpiresAt;
        return this;
    }
}