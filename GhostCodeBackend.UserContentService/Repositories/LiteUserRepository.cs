using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.UserContentService.Repositories;

public class LiteUserRepository : ILiteUserRepository
{
    private readonly IMongoDatabase _db;

    private readonly IMongoCollection<User> _users;

    public LiteUserRepository(IMongoDatabase db)
    {
        _db = db;
        db.CreateCollection("users");
        
        var indexKeys =  Builders<User>.IndexKeys.Ascending(u => u.Login);
        var indexModel = new CreateIndexModel<User>(indexKeys, new CreateIndexOptions{ Unique = true });
        
        _users = db.GetCollection<User>("users");
        
        _users.Indexes.CreateOne(indexModel);
    }

    public async Task<User> GetUser(string userid)
    {
        return await _users.Find(u => u.Id == userid).FirstOrDefaultAsync();
    }

    public async Task<(bool result, User? updatedUser)> UpdateUser(User user)
    {
        try
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            return (true, user);
        }
        catch (Exception e)
        {
            return (false, null);
        }
    }
}