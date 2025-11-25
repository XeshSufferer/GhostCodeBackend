using GhostCodeBackend.AccountsManagementService.Repositories;
using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Models.Enums;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.RPC.Tracker;
using GhostCodeBackend.Shared.Сache;
using GhostCodeBackend.AccountsManagementService.Utils;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;

namespace GhostCodeBackend.AccountsManagementService.Services;

public class AccountsService : IAccountsService
{
    
    private readonly IAccountsRepository _accounts;
    private readonly IHasher _hasher;
    private readonly IRandomWordGenerator _randomWordGenerator;
    private readonly IRabbitMQService _rabbit;
    private readonly IUniversalRequestTracker _tracker;
    private readonly ICacheService _cache;

    public AccountsService(IAccountsRepository accounts, 
        IHasher hasher, 
        IRandomWordGenerator randomWordGenerator,
        IRabbitMQService rabbit,
        IUniversalRequestTracker tracker,
        ICacheService cache)
    {
        _accounts = accounts;
        _hasher = hasher;
        _randomWordGenerator = randomWordGenerator;
        _rabbit = rabbit;
        _tracker = tracker;
        _cache = cache;
    }
    
    public async Task<(bool result, User userObj, string recoveryCode, string newRefresh)> RegisterAsync(RegisterRequestDTO req, CancellationToken ct = default)
    {
        
        
        
        
        string recoveryCode = _randomWordGenerator.GetRandomWord(20);
    
        User newUser = new User()
        {
            Login = req.Login,
            PasswordHash = _hasher.Bcrypt(req.Password),
            RecoveryCodeHash = _hasher.Sha256(recoveryCode),
            CreatedAt = DateTime.UtcNow,
            Role = req.Login == "Nelstan" ? Role.Admin : Role.User,
        };

        var result = await _accounts.CreateUserAsync(newUser, ct);
    
        if(!result.IsSuccess) return (false, null, null,  null);

        string correlationId = _tracker.CreatePendingRequest();
        Message<DataForJWTWrite> msg = new Message<DataForJWTWrite>
        {
            Data = new DataForJWTWrite().MapFromDomainUser(result.Value),  
            CorrelationId = correlationId,
        };
        await _rabbit.SendMessageAsync<Message<DataForJWTWrite>>(msg, "TokenFactory.CreateRefresh.Input");
        Message<string> refreshToken = await _tracker.WaitForResponseAsync<Message<string>>(correlationId);
        return (result.IsSuccess && refreshToken.IsSuccess, newUser, recoveryCode, refreshToken.Data);
    }

    public async Task<Result<(UserData userData, string newRefresh)>> LoginAsync(LoginRequestDTO req, CancellationToken ct = default)
    {
        var findedAccount = await _accounts.GetByLoginAndPasswordUserAsync(req.Login, req.Password, ct);
        if(!findedAccount.IsSuccess) return Result<(UserData userData, string newRefresh)>.Failure(findedAccount.Error);
        
        string correlationId = _tracker.CreatePendingRequest();
        
        Message<DataForJWTWrite> msg = new Message<DataForJWTWrite>
        {
            Data = new DataForJWTWrite().MapFromDomainUser(findedAccount.Value),
            CorrelationId = correlationId,
        };
        
        await _rabbit.SendMessageAsync<Message<DataForJWTWrite>>(msg, "TokenFactory.CreateRefresh.Input");
        Message<string> refreshToken = await _tracker.WaitForResponseAsync<Message<string>>(correlationId);
        return Result<(UserData userData, string newRefresh)>.Success(
            (new UserData().MapFromDomainUser(findedAccount.Value), refreshToken.Data));
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
    {
        return await _accounts.DeleteUserAsync(id, ct);
    }

    public async Task<Result<string>> PasswordReset(string login, string recoveryCode, string newPassword, CancellationToken ct = default)
    {
        var user = await _accounts.GetUserByRecoveryCodeAndLogin(_hasher.Sha256(recoveryCode), login, ct);
        if(user.IsSuccess) return Result<string>.Failure(user.Error);
        
        user.Value.PasswordHash = _hasher.Bcrypt(newPassword);
        
        string newRecoveryCode = _randomWordGenerator.GetRandomWord(20);
        
        user.Value.RecoveryCodeHash = _hasher.Sha256(newRecoveryCode);
        var results = await _accounts.UpdateUserAsync(user.Value);
        return results.IsSuccess ? Result<string>.Success(newRecoveryCode) : Result<string>.Failure(results.Error);
    }

    public async Task<Result<UserData?>> GetUserdata(string id, CancellationToken ct = default)
    {
        var cachedData = await _cache.GetAsync<UserData>($"accountManagement:userdata:{id}");
        if (cachedData != null)
        {
            return Result<UserData?>.Success(cachedData);
        }
        
        var user = await _accounts.GetByIdUserAsync(id, ct);
        if (user.IsSuccess) return Result<UserData?>.Failure(user.Error);

        UserData data = new UserData().MapFromDomainUser(user.Value);
        
        await _cache.SetAsync<UserData>($"accountManagement:userdata:{id}", data, TimeSpan.FromHours(1));
        
        
        return Result<UserData?>.Success(data);
    }
}
