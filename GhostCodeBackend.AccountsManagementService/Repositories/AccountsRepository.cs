using MongoDB.Driver;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.AccountsManagementService.Utils;
using MongoDB.Driver.Linq;

namespace GhostCodeBackend.AccountsManagementService.Repositories;

public class AccountsRepository : IAccountsRepository
{
    private readonly IMongoDatabase _db;

    private readonly IMongoCollection<User> _users;
    private readonly IHasher _hasher;
    private readonly ILogger<AccountsRepository> _logger;

    public AccountsRepository(IMongoDatabase db, IHasher hasher, ILogger<AccountsRepository> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
        db.CreateCollection("users");
        
        var indexKeys =  Builders<User>.IndexKeys.Ascending(u => u.Login);
        var indexModel = new CreateIndexModel<User>(indexKeys, new CreateIndexOptions{ Unique = true });
        
        _users = db.GetCollection<User>("users");
        
        _users.Indexes.CreateOne(indexModel);
    }

    public async Task<Result<User?>> GetByIdUserAsync(string id, CancellationToken ct = default)
    {
        try
        {
            
            var user = await _users.AsQueryable().Where(u => u.Id == id).FirstOrDefaultAsync(ct);
            return user != null ? Result<User?>.Success(user) : Result<User?>.Failure("User not fount");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Error by finding user with id {id}", id);
            return Result<User?>.Failure(e.Message);
        }
    }

    public async Task<Result<User>> CreateUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            await _users.InsertOneAsync(user, ct);
            return Result<User>.Success(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<User>.Failure(e.Message);
        }
    }

    public async Task<Result> UpdateUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            var existingUser = await _users.Find(u => u.Id == user.Id).FirstOrDefaultAsync(ct);
            if (existingUser == null)
            {
                return Result.Failure("User not found");
            }
            
            var opts = new ReplaceOptions { IsUpsert = false };
            var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user, opts, ct);
            
            return result.IsAcknowledged && result.ModifiedCount > 0 ? Result.Success() : Result.Failure("User not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> DeleteUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            var deleteResult = await _users.DeleteOneAsync(u => u.Id == user.Id, ct);
            return deleteResult.DeletedCount > 0 ? Result.Success() : Result.Failure("User not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Failure(e.Message);
        }
    }
    
    public async Task<Result> DeleteUserAsync(string userid, CancellationToken ct = default)
    {
        try
        {
            var deleteResult = await _users.DeleteOneAsync(u => u.Id == userid, ct);
            return deleteResult.DeletedCount > 0 ? Result.Success() : Result.Failure("User not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result<User?>> GetByLoginAndPasswordUserAsync(string login, string password, CancellationToken ct = default)
    {
        var user = await _users.AsQueryable()
            .Where(u => u.Login == login)
            .FirstOrDefaultAsync(ct);

        if (user == null || !_hasher.VerifyBcrypt(user.PasswordHash, password))
            return Result<User?>.Failure("Invalid login or password");

        return Result<User?>.Success(user);
    }

    public async Task<Result<User?>> GetUserByRecoveryCodeAndLogin(string recoveryCodeHash, string login,
        CancellationToken ct = default)
    {
        try
        {
            var user = await _users.AsQueryable().Where(u => u.Login == login && u.RecoveryCodeHash == recoveryCodeHash).FirstOrDefaultAsync(ct);
            return user != null ?  Result<User?>.Success(user) : Result<User?>.Failure("User not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result<User?>.Failure(e.Message);
        }
    }




}