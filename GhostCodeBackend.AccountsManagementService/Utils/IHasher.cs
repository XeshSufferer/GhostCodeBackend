namespace GhostCodeBakend.AccountsManagementService.Utils;

public interface IHasher
{
    string Bcrypt(string password);
    bool VerifyBcrypt(string hash, string password);
    string Sha256(string input);
}