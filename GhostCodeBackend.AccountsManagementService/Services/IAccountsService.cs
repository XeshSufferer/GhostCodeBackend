using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBakend.AccountsManagementService.Services;

public interface IAccountsService
{
    Task<(bool result, User userObj, string recoveryCode, string newRefresh)> RegisterAsync(RegisterRequestDTO req, CancellationToken ct = default);
    Task<(bool result, UserData userData, string newRefresh)>  LoginAsync(LoginRequestDTO req, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    Task<(bool result, string newRecoveryCode)>  PasswordReset(string login, string recoveryCode, string newPassword,
        CancellationToken ct = default);
}