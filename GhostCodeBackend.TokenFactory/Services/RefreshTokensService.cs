using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Models.Enums;
using TokenFactory.Repositories;

namespace TokenFactory.Services;

public class RefreshTokensService : IRefreshTokensService
{
    
    private readonly int _daysBeforeExpiryRefresh = 30;
    private readonly IRefreshTokensRepository _refreshRepository;

    public RefreshTokensService(IRefreshTokensRepository repository, int daysBeforeExpiryRefresh)
    {
        _refreshRepository = repository;
        _daysBeforeExpiryRefresh = daysBeforeExpiryRefresh;
    }

    public async Task<Result<RefreshToken?>> ValidAndNotExpired(string token)
    {
        var findedToken = await _refreshRepository.Get(token);
        if(!findedToken.IsSuccess) return Result<RefreshToken?>.Failure(findedToken.Error);
        if ((DateTime.UtcNow.Date - findedToken.Value.ExpiresAt.Date).TotalDays > _daysBeforeExpiryRefresh)
        {
            await _refreshRepository.Delete(findedToken.Value);
            return Result<RefreshToken?>.Failure("Token is expired");
        }

        return Result<RefreshToken?>.Success(findedToken.Value);
    }

    public async Task<Result<RefreshToken>> RotateToken(RefreshToken token)
    {
        
        
        RefreshToken newToken = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromDays(_daysBeforeExpiryRefresh),
            Token = Guid.NewGuid().ToString(),
            UserId = token.UserId
        };
        
        var result = await _refreshRepository.RotateToken(token, newToken);
        
        return result.IsSuccess ? Result<RefreshToken>.Success(newToken) : Result<RefreshToken>.Failure(result.Error);
    }

    public async Task<Result> KillToken(string token)
    {
        return await _refreshRepository.Delete(token);
    }

    public async Task<Result<RefreshToken?>> CreateToken(DataForJWTWrite userData)
    {
        RefreshToken token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromDays(_daysBeforeExpiryRefresh),
            Token = Guid.NewGuid().ToString(),
            UserId = userData.Id,
            Role = userData.Role,
            Tier = userData.SubscribeExpiresAt < DateTime.UtcNow ? SubscriptionTier.None : userData.Tier
        };
        
        var result = await _refreshRepository.Insert(token);
        
        
        return result.IsSuccess ? Result<RefreshToken?>.Success(token) : Result<RefreshToken?>.Failure(result.Error);
    }
}