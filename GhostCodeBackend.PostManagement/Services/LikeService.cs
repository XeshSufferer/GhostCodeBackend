using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public class LikeService : ILikeService
{

    private readonly IPostsRepository _posts;
    
    public LikeService(IPostsRepository posts)
    {
        _posts = posts;
    }

    public async Task<Result> Like(string postId, string userId, CancellationToken ct = default)
    {
        return await _posts.Like(postId, userId, ct);
    }
}