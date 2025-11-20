using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.AccountsManagementService.Services;

public interface IAccountsService
{
    Task<(bool result, User userObj, string recoveryCode, string newRefresh)> RegisterAsync(RegisterRequestDTO req, CancellationToken ct = default);
    Task<Result<(UserData userData, string newRefresh)>>  LoginAsync(LoginRequestDTO req, CancellationToken ct = default);
    Task<Result> DeleteAsync(string id, CancellationToken ct = default);

    Task<Result<string>>  PasswordReset(string login, string recoveryCode, string newPassword,
        CancellationToken ct = default);

    Task<Result<UserData?>> GetUserdata(string id, CancellationToken ct = default);
}