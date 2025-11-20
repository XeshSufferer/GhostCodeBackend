using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.AccountsManagementService.Repositories;

public interface IAccountsRepository
{
    Task<Result<User>> CreateUserAsync(User user, CancellationToken ct = default);
    Task<Result<User?>> GetByIdUserAsync(string id, CancellationToken ct = default);
    Task<Result> UpdateUserAsync(User user, CancellationToken ct = default);
    Task<Result> DeleteUserAsync(User user, CancellationToken ct = default);
    Task<Result<User?>> GetByLoginAndPasswordUserAsync(string login, string password, CancellationToken ct = default);
    Task<Result> DeleteUserAsync(string userid, CancellationToken ct = default);

    Task<Result<User?>> GetUserByRecoveryCodeAndLogin(string recoveryCode, string login,
        CancellationToken ct = default);

}