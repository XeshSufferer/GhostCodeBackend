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
        _tokens = db.GetCollection<RefreshToken>("refresh_tokens");
    }

    public async Task<RefreshToken?> Get(string token)
    {
        return await _tokens.AsQueryable().Where(t => t.Token == token).FirstOrDefaultAsync();
    }

    public async Task<bool> Insert(RefreshToken token)
    {
        try
        {
            await _tokens.InsertOneAsync(token);
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