using GhostCodeBackend.Shared.Models;

namespace TokenFactory.Repositories;

public interface IRefreshTokensRepository
{
    Task<RefreshToken?> Get(string token);
    Task<bool> Insert(RefreshToken token);
    Task<bool> Delete(RefreshToken token);
    Task<bool> Delete(string token);
    Task<bool> RotateToken(RefreshToken oldToken, RefreshToken newToken);
    Task<bool> RotateToken(string oldToken, RefreshToken newToken);
}