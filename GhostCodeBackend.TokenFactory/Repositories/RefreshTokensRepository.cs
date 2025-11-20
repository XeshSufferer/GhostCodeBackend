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

    public async Task<Result<RefreshToken?>> Get(string token)
    {
        try
        {
            var foundedToken = await _tokens.AsQueryable().Where(t => t.Token == token).FirstOrDefaultAsync();
            return token != null ? Result<RefreshToken?>.Success(foundedToken) :  Result<RefreshToken?>.Failure("Token not found");
        }
        catch (Exception e)
        {
            return Result<RefreshToken?>.Failure(e.Message);
        }
    }

    public async Task<Result<RefreshToken?>> GetByUserId(string userId)
    {
        try
        {
            var token = await _tokens.AsQueryable().Where(t => t.UserId == userId).FirstOrDefaultAsync();
            return token != null ? Result<RefreshToken?>.Success(token) :  Result<RefreshToken?>.Failure("Token not found");
        }
        catch (Exception e)
        {
            return Result<RefreshToken?>.Failure(e.Message);
        }
    }

    public async Task<Result> Insert(RefreshToken token)
    {
        try
        {
            var opts = new ReplaceOptions { IsUpsert = true };
            await _tokens.ReplaceOneAsync(u => u.Id == token.Id || token.UserId == u.UserId, token, opts);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> Delete(RefreshToken token)
    {
        try
        {
            var result = await _tokens.DeleteOneAsync(t => t.Id == token.Id);
            return result.DeletedCount > 0 ? Result.Success() : Result.Failure("Token not found");
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
    
    public async Task<Result> Delete(string token)
    {
        try
        {
            var result = await _tokens.DeleteOneAsync(t => t.Token == token);
            return result.DeletedCount > 0 ? Result.Success() : Result.Failure("Token not found");
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> RotateToken(RefreshToken oldToken, RefreshToken newToken)
    {

        try
        {
            var deleteResult = await _tokens.DeleteOneAsync(t => t.Id == oldToken.Id);
            await _tokens.InsertOneAsync(newToken);
            return deleteResult.DeletedCount > 0 ? Result.Success() : Result.Failure("Rotated token not found");
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
    
    public async Task<Result> RotateToken(string oldToken, RefreshToken newToken)
    {
        try
        {
            var deleteResult = await _tokens.DeleteOneAsync(t => t.Token == oldToken);
            await _tokens.InsertOneAsync(newToken);
            return deleteResult.DeletedCount > 0 ? Result.Success() : Result.Failure("Rotated token not found");
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
}