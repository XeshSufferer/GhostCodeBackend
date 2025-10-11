using GhostCodeBackend.AccountsManagementService.Repositories;
using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.RPC.Tracker;
using GhostCodeBakend.AccountsManagementService.Utils;
using MongoDB.Bson;

namespace GhostCodeBakend.AccountsManagementService.Services;

public class AccountsService : IAccountsService
{
    
    private readonly IAccountsRepository _accounts;
    private readonly IHasher _hasher;
    private readonly IRandomWordGenerator _randomWordGenerator;
    private readonly IRabbitMQService _rabbit;
    private readonly IUniversalRequestTracker _tracker;

    public AccountsService(IAccountsRepository accounts, 
        IHasher hasher, 
        IRandomWordGenerator randomWordGenerator,
        IRabbitMQService rabbit,
        IUniversalRequestTracker tracker)
    {
        _accounts = accounts;
        _hasher = hasher;
        _randomWordGenerator = randomWordGenerator;
        _rabbit = rabbit;
        _tracker = tracker;
    }
    
    public async Task<(bool result, User userObj, string recoveryCode, string newRefresh)> RegisterAsync(RegisterRequestDTO req, CancellationToken ct = default)
    {
        string recoveryCode = _randomWordGenerator.GetRandomWord(20);
    
        User newUser = new User()
        {
            Login = req.Login,
            PasswordHash = _hasher.Bcrypt(req.Password),
            RecoveryCodeHash = _hasher.Sha256(recoveryCode),
        };

        var result = await _accounts.CreateUserAsync(newUser, ct);
    
        if(!result.Item1) return (false, null, null,  null);

        string correlationId = _tracker.CreatePendingRequest();
        Message<string> msg = new Message<string>
        {
            Data = result.Item2.Id,  
            CorrelationId = correlationId,
        };
        await _rabbit.SendMessageAsync<Message<string>>(msg, "TokenFactory.CreateRefresh.Input");
        Message<string> refreshToken = await _tracker.WaitForResponseAsync<Message<string>>(correlationId);
        return (result.Item1 && refreshToken.IsSuccess, newUser, recoveryCode, refreshToken.Data);
    }

    public async Task<(bool result, UserData userData, string newRefresh)> LoginAsync(LoginRequestDTO req, CancellationToken ct = default)
    {
        User? findedAccount = await _accounts.GetByLoginAndPasswordUserAsync(req.Login, req.Password, ct);
        if(findedAccount == null) return (false, null, null);
        
        string correlationId = _tracker.CreatePendingRequest();
        
        Message<string> msg = new Message<string>
        {
            Data = findedAccount.Id,
            CorrelationId = correlationId,
        };
        
        await _rabbit.SendMessageAsync<Message<string>>(msg, "TokenFactory.CreateRefresh.Input");
        Message<string> refreshToken = await _tracker.WaitForResponseAsync<Message<string>>(correlationId);
        return (refreshToken.IsSuccess, new UserData(), refreshToken.Data);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        return await _accounts.DeleteUserAsync(id, ct);
    }

    public async Task<(bool result, string newRecoveryCode)> PasswordReset(string login, string recoveryCode, string newPassword, CancellationToken ct = default)
    {
        User? user = await _accounts.GetUserByRecoveryCodeAndLogin(_hasher.Sha256(recoveryCode), login, ct);
        //if(user == null) return (false, null);
        
        user.PasswordHash = _hasher.Bcrypt(newPassword);
        
        string newRecoveryCode = _randomWordGenerator.GetRandomWord(20);
        
        user.RecoveryCodeHash = _hasher.Sha256(newRecoveryCode);
        bool results = await _accounts.UpdateUserAsync(user);
        return (results, newRecoveryCode);
    }
}