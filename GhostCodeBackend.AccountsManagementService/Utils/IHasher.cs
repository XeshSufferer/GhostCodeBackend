namespace GhostCodeBakend.AccountsManagementService.Utils;

public interface IHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}