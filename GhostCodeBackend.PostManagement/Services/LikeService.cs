using GhostCodeBackend.PostManagement.Repositories;

namespace GhostCodeBackend.PostManagement.Services;

public class LikeService : ILikeService
{

    private readonly IColdLikesRepository _coldLikesRepository;
    
    public LikeService(IColdLikesRepository coldLikesRepository)
    {
        _coldLikesRepository = coldLikesRepository;
    }

    public async Task<bool> Like(string postId, string userId, CancellationToken ct = default)
    {
        return await _coldLikesRepository.Like(postId, userId, ct);
    }
}