using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GhostCodeBackend.Shared.Models;
using Microsoft.IdentityModel.Tokens;
using Shared.CfgObjects;

namespace TokenFactory.Services;

public class JwtService : IJwtService
{

    private readonly JwtOptions _options;
    private readonly IRefreshTokensService _refresher;
    private readonly ILogger<JwtService> _logger;

    public JwtService(JwtOptions options, IRefreshTokensService refresher, ILogger<JwtService> logger)
    {
        _options = options;
        _refresher = refresher;
        _logger = logger;
    }

    public async Task<Result<TokenPair>> CreateToken(string token)
    {
        var validateResult = await _refresher.ValidAndNotExpired(token);
        _logger.LogInformation("Validated token: {token} | result: {result}", token, validateResult.IsSuccess);
        if (!validateResult.IsSuccess) return Result<TokenPair>.Failure(validateResult.Error);
        var rotateResult = await _refresher.RotateToken(validateResult.Value);
        _logger.LogInformation("Rotated token: {token} | result: {result} | newToken: {newToken}", token, rotateResult.IsSuccess, rotateResult.Value.UserId);
        if (rotateResult.IsSuccess)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newjwt = new JwtSecurityToken(
                audience: _options.Audience,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
                issuer: _options.Issuer,
                signingCredentials: creds,
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, validateResult.Value.UserId),
                    new Claim(JwtRegisteredClaimNames.Jti, validateResult.Value.UserId),
                    new Claim(ClaimTypes.Role,  validateResult.Value.Role.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, validateResult.Value.Tier.ToString()),
                }
            );

            var newJwtWrited = new JwtSecurityTokenHandler().WriteToken(newjwt);
            
            return Result<TokenPair>.Success(new TokenPair(newJwtWrited, rotateResult.Value.Token));
        }
        return Result<TokenPair>.Failure(rotateResult.Error);
    }
}