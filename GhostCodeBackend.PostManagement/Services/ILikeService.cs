namespace GhostCodeBackend.PostManagement.Services;

public interface ILikeService
{
    Task<bool> Like(string postId, string userId, CancellationToken ct = default);
}