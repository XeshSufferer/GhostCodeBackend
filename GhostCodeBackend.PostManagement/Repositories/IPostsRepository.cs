using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Repositories;

public interface IPostsRepository
{
    Task<(bool result, List<Post>? posts)> GetLastPostsAsync(int count, CancellationToken ct = default);
    Task<(bool result, Post? post)> PostAsync(Post post, CancellationToken ct = default);
}