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

    public async Task<(bool result, string newJwt, string newRefresh)> CreateToken(string token)
    {
        var validateResult = await _refresher.ValidAndNotExpired(token);
        _logger.LogInformation("Validated token: {token} | result: {result}", token, validateResult.result);
        if (!validateResult.result) return (false, "", "");
        var rotateResult = await _refresher.RotateToken(validateResult.token);
        _logger.LogInformation("Rotated token: {token} | result: {result} | newToken: {newToken}", token, rotateResult.result, rotateResult.newToken.UserId);
        if (rotateResult.result)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newjwt = new JwtSecurityToken(
                audience: _options.Audience,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
                issuer: _options.Issuer,
                signingCredentials: creds,
                claims: new[] { new Claim(ClaimTypes.Name, validateResult.token.UserId) }
            );

            return (true, 
                new JwtSecurityTokenHandler().WriteToken(newjwt), 
                rotateResult.newToken.Token);
        }
        return (false, "", "");
    }
}