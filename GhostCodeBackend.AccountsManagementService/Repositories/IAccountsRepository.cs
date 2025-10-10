using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.AccountsManagementService.Repositories;

public interface IAccountsRepository
{
    Task<bool> CreateUserAsync(User user, CancellationToken ct = default);
    Task<User?> GetByIdUserAsync(string id, CancellationToken ct = default);
    Task<bool> UpdateUserAsync(User user, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(User user, CancellationToken ct = default);
    Task<User?> GetByLoginAndPasswordUserAsync(string login, string password, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(string userid, CancellationToken ct = default);

}