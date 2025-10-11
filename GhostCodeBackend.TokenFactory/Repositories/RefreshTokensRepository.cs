using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace TokenFactory.Repositories;

public class RefreshTokensRepository : IRefreshTokensRepository
{

    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<RefreshToken> _tokens;

    public RefreshTokensRepository(IMongoDatabase db)
    {
        _db = db;
        
        _db.CreateCollection("refresh_tokens");
        
        var indexKeys =  Builders<RefreshToken>.IndexKeys
            .Ascending(t => t.Token)
            .Ascending(t => t.UserId);
        var indexModel = new CreateIndexModel<RefreshToken>(indexKeys, new CreateIndexOptions{ Unique = false });
        
        _tokens = db.GetCollection<RefreshToken>("refresh_tokens");
        _tokens.Indexes.CreateOne(indexModel);
    }

    public async Task<RefreshToken?> Get(string token)
    {
        return await _tokens.AsQueryable().Where(t => t.Token == token).FirstOrDefaultAsync();
    }

    public async Task<RefreshToken?> GetByUserId(string userId)
    {
        return await _tokens.AsQueryable().Where(t => t.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<bool> Insert(RefreshToken token)
    {
        try
        {
            var opts = new ReplaceOptions { IsUpsert = true };
            await _tokens.ReplaceOneAsync(u => u.Id == token.Id || token.UserId == u.UserId, token, opts);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<bool> Delete(RefreshToken token)
    {
        try
        {
            await _tokens.DeleteOneAsync(t => t.Id == token.Id);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    public async Task<bool> Delete(string token)
    {
        try
        {
            await _tokens.DeleteOneAsync(t => t.Token == token);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<bool> RotateToken(RefreshToken oldToken, RefreshToken newToken)
    {

        try
        {
            await _tokens.DeleteOneAsync(t => t.Id == oldToken.Id);
            await _tokens.InsertOneAsync(newToken);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    public async Task<bool> RotateToken(string oldToken, RefreshToken newToken)
    {
        try
        {
            await _tokens.DeleteOneAsync(t => t.Token == oldToken);
            await _tokens.InsertOneAsync(newToken);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}