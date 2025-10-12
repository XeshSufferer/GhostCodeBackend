using GhostCodeBackend.Shared.Models.Enums;

namespace GhostCodeBackend.Shared.Models;

public class UserData
{
    public string Name { get; set; }
    public string Id { get; set; }
    public Role Role { get; set; }
    public SubscriptionTier Tier { get; set; }
    
    public string Description { get; set; }
    public string AvatarLink { get; set; }
    public string HeaderLink { get; set; }

    public UserData MapFromDomainUser(User user)
    {
        Name = user.Login;
        Id = user.Id;
        Role = user.Role;
        Tier = user.Tier;
        Description = user.Description;
        AvatarLink = user.AvatarLink;
        HeaderLink = user.HeaderLink;
        return this;
    }
}