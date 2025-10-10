using GhostCodeBackend.AccountsManagementService.Repositories;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;
using GhostCodeBakend.AccountsManagementService.Utils;

namespace GhostCodeBakend.AccountsManagementService.Services;

public class AccountsService : IAccountsService
{
    
    private readonly IAccountsRepository _accounts;
    private readonly IHasher _hasher;
    private readonly IRandomWordGenerator _randomWordGenerator;

    public AccountsService(IAccountsRepository accounts, IHasher hasher, IRandomWordGenerator randomWordGenerator)
    {
        _accounts = accounts;
        _hasher = hasher;
        _randomWordGenerator = randomWordGenerator;
    }
    
    public async Task<(bool, User, string)> RegisterAsync(RegisterRequestDTO req, CancellationToken ct = default)
    {
        string recoveryCode = _randomWordGenerator.GetRandomWord(20);
        
        User newUser = new User()
        {
            Login = req.Login,
            PasswordHash = _hasher.Hash(req.Password),
            RecoveryCodeHash = _hasher.Hash(recoveryCode),
        };

        bool result = await _accounts.CreateUserAsync(newUser, ct);
        
        return (result, newUser, recoveryCode);
    }

    public async Task<(bool, UserData)> LoginAsync(LoginRequestDTO req, CancellationToken ct = default)
    {
        User? findedAccount = await _accounts.GetByLoginAndPasswordUserAsync(req.Login, req.Password, ct);
        return (findedAccount != null, new UserData());
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        return await _accounts.DeleteUserAsync(id, ct);
    }

    public async Task<(bool, string)> PasswordReset(string login, string recoveryCode, string newPassword, CancellationToken ct = default)
    {
        User? user = await _accounts.GetUserByRecoveryCodeAndLogin(recoveryCode, login, ct);
        user.PasswordHash = _hasher.Hash(newPassword);
        
        string newRecoveryCode = _randomWordGenerator.GetRandomWord(20);
        
        user.RecoveryCodeHash = _hasher.Hash(newRecoveryCode);
        bool results = await _accounts.UpdateUserAsync(user);
        return (results, newRecoveryCode);
    }
}