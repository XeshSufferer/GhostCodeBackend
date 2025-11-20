using GhostCodeBackend.Shared.Models;

namespace TokenFactory.Services;

public interface IJwtService
{
    Task<Result<TokenPair>> CreateToken(string token);
}