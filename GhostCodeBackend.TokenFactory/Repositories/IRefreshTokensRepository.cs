using GhostCodeBackend.Shared.Models;

namespace TokenFactory.Repositories;

public interface IRefreshTokensRepository
{
    Task<Result<RefreshToken?>> Get(string token);
    Task<Result> Insert(RefreshToken token);
    Task<Result> Delete(RefreshToken token);
    Task<Result> Delete(string token);
    Task<Result> RotateToken(RefreshToken oldToken, RefreshToken newToken);
    Task<Result> RotateToken(string oldToken, RefreshToken newToken);
}