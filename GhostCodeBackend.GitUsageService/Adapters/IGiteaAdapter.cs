using GhostCodeBackend.Shared.DTO.Requests;

namespace GhostCodeBackend.GitUsageService.Adapters;

public interface IGiteaAdapter
{
    Task<bool> TryCreateRepository(GitRepositoryCreateRequestDTO req);
    Task<bool> CreateAccount(RegisterRequestDTO req);
}