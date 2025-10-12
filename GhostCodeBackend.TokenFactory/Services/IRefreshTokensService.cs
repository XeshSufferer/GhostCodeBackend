using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.Models;

namespace TokenFactory.Services;

public interface IRefreshTokensService
{
    Task<(bool result, RefreshToken? token)> ValidAndNotExpired(string token);
    Task<(bool result, RefreshToken newToken)> RotateToken(RefreshToken token);
    Task<bool> KillToken(string token);
    Task<(bool result, RefreshToken? token)> CreateToken(DataForJWTWrite userData);
}