using GhostCodeBackend.Shared.DTO.Requests;

namespace GhostCodeBackend.GitUsageService.Services;

public interface IGitUsageService
{
    Task<bool> TryCreateRepository(GitRepositoryCreateRequestDTO req);
    Task<bool> TryCreateAccount(RegisterRequestDTO req);
}