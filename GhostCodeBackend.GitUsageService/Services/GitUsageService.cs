using GhostCodeBackend.GitUsageService.Adapters;
using GhostCodeBackend.Shared.DTO.Requests;

namespace GhostCodeBackend.GitUsageService.Services;

public class GitUsageService : IGitUsageService
{

    private readonly IGiteaAdapter _adapter;
    
    public GitUsageService(IGiteaAdapter adapter)
    {
        _adapter = adapter;
    }


    public async Task<bool> TryCreateRepository(GitRepositoryCreateRequestDTO req)
    {
        return await _adapter.TryCreateRepository(req);
    }
    
    public async Task<bool> TryCreateAccount(RegisterRequestDTO req)
    {
        return await _adapter.CreateAccount(req);
    }
}