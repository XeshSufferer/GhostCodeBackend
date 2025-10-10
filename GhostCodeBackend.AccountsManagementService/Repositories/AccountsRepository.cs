using MongoDB.Driver;
using GhostCodeBackend.Shared.Models;
using MongoDB.Driver.Linq;

namespace GhostCodeBackend.AccountsManagementService.Repositories;

public class AccountsRepository : IAccountsRepository
{
    private readonly IMongoDatabase _db;

    private readonly IMongoCollection<User> _users;

    public AccountsRepository(IMongoDatabase db)
    {
        _db = db;
        db.CreateCollection("accounts");
        _users = db.GetCollection<User>("accounts");
    }

    public async Task<User?> GetByIdUserAsync(string id, CancellationToken ct = default)
    {
        return await _users.AsQueryable().Where(u => u.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CreateUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            await _users.InsertOneAsync(user, ct);
            return true;
        }
        catch (MongoException e)
        {
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(User user, CancellationToken ct = default)
    {
        try
        {
            var opts = new ReplaceOptions { IsUpsert = false };
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user, opts, ct);
            return true;
        }
        catch (MongoException e)
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
        catch (MongoException e)
        {
            return false;
        }
    }




}