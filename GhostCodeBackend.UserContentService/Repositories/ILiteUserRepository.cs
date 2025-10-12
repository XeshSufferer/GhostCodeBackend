using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.UserContentService.Repositories;

public interface ILiteUserRepository
{
    Task<User> GetUser(string userid);
    Task<(bool result, User? updatedUser)> UpdateUser(User user);
}