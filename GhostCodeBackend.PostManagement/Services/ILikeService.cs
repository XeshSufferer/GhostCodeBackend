using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface ILikeService
{
    Task<Result> Like(string postId, string userId, CancellationToken ct = default);
}