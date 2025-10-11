using GhostCodeBackend.Shared.Models;
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

    public async Task<(bool result, RefreshToken? token)> ValidAndNotExpired(string token)
    {
        RefreshToken? findedToken = await _refreshRepository.Get(token);
        if(findedToken == null) return (false, null);
        if ((DateTime.UtcNow.Date - findedToken.ExpiresAt.Date).TotalDays > _daysBeforeExpiryRefresh)
        {
            await _refreshRepository.Delete(findedToken);
            return (false, null);
        }

        return (true, findedToken);
    }

    public async Task<(bool result, RefreshToken newToken)> RotateToken(RefreshToken token)
    {
        
        
        RefreshToken newToken = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromDays(_daysBeforeExpiryRefresh),
            Token = Guid.NewGuid().ToString(),
            UserId = token.UserId
        };
        
        var result = await _refreshRepository.RotateToken(token, newToken);
        
        return (result, newToken);
    }

    public async Task<bool> KillToken(string token)
    {
        return await _refreshRepository.Delete(token);
    }

    public async Task<(bool result, RefreshToken? token)> CreateToken(string userid)
    {
        RefreshToken token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromDays(_daysBeforeExpiryRefresh),
            Token = Guid.NewGuid().ToString(),
            UserId = userid
        };
        
        var result = await _refreshRepository.Insert(token);
        
        return (result, result ? token : null);
    }
}