using MongoDB.Driver;
using GhostCodeBackend.Shared.Models;
using GhostCodeBakend.AccountsManagementService.Utils;
using MongoDB.Driver.Linq;

namespace GhostCodeBackend.AccountsManagementService.Repositories;

public class AccountsRepository : IAccountsRepository
{
    private readonly IMongoDatabase _db;

    private readonly IMongoCollection<User> _users;
    private readonly IHasher _hasher;

    public AccountsRepository(IMongoDatabase db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
        db.CreateCollection("users");
        
        var indexKeys =  Builders<User>.IndexKeys.Ascending(u => u.Login);
        var indexModel = new CreateIndexModel<User>(indexKeys, new CreateIndexOptions{ Unique = true });
        
        _users = db.GetCollection<User>("users");
        
        _users.Indexes.CreateOne(indexModel);
    }

    public async Task<User?> GetByIdUserAsync(string id, CancellationToken ct = default)
    {
        try
        {
            return await _users.AsQueryable().Where(u => u.Id == id).FirstOrDefaultAsync(ct);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<(bool, User)> CreateUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            await _users.InsertOneAsync(user, ct);
            return (true, user);
        }
        catch (Exception e)
        {
            return (false, null);
        }
    }

    public async Task<bool> UpdateUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            var existingUser = await _users.Find(u => u.Id == user.Id).FirstOrDefaultAsync(ct);
            if (existingUser == null)
            {
                return false;
            }
            
            var opts = new ReplaceOptions { IsUpsert = false };
            var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user, opts, ct);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            await _users.DeleteOneAsync(u => u.Id == user.Id, ct);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    public async Task<bool> DeleteUserAsync(string userid, CancellationToken ct = default)
    {
        try
        {
            await _users.DeleteOneAsync(u => u.Id == userid, ct);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<User?> GetByLoginAndPasswordUserAsync(string login, string password, CancellationToken ct = default)
    {
        var user = await _users.AsQueryable()
            .Where(u => u.Login == login)
            .FirstOrDefaultAsync(ct);

        if (user == null || !_hasher.VerifyBcrypt(user.PasswordHash, password))
            return null;

        return user;
    }

    public async Task<User?> GetUserByRecoveryCodeAndLogin(string recoveryCode, string login,
        CancellationToken ct = default)
    {
        return await _users.AsQueryable().Where(u => u.Login == login && u.RecoveryCodeHash == recoveryCode).FirstOrDefaultAsync(ct);
    }




}