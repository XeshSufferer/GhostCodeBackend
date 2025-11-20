namespace GhostCodeBackend.Shared.Models;

public class TokenPair
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    
    public TokenPair(string token, string refreshToken)
    {
        Token = token;
        RefreshToken = refreshToken;
    }
}