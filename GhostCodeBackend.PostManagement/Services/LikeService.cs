using GhostCodeBackend.PostManagement.Repositories;

namespace GhostCodeBackend.PostManagement.Services;

public class LikeService : ILikeService
{

    private readonly IPostsRepository _posts;
    
    public LikeService(IPostsRepository posts)
    {
        _posts = posts;
    }

    public async Task<bool> Like(string postId, string userId, CancellationToken ct = default)
    {
        return await _posts.Like(postId, userId, ct);
    }
}