using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.Models;

namespace TokenFactory.Services;

public interface IRefreshTokensService
{
    Task<Result<RefreshToken?>> ValidAndNotExpired(string token);
    Task<Result<RefreshToken>> RotateToken(RefreshToken token);
    Task<Result> KillToken(string token);
    Task<Result<RefreshToken?>> CreateToken(DataForJWTWrite userData);
}