using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBakend.AccountsManagementService.Services;

public interface IAccountsService
{
    Task<(bool, User, string)> RegisterAsync(RegisterRequestDTO req, CancellationToken ct = default);
    Task<(bool, UserData)> LoginAsync(LoginRequestDTO req, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    Task<(bool, string)> PasswordReset(string login, string recoveryCode, string newPassword,
        CancellationToken ct = default);
}