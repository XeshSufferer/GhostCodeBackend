namespace TokenFactory.Services;

public interface IJwtService
{
    Task<(bool result, string newJwt, string newRefresh)> CreateToken(string token);
}