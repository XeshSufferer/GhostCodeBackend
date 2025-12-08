using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface ILikeService
{
    Task<Result> Like(int postId, string userId, CancellationToken ct = default);
}