using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Сache;
using MongoDB.Driver;

namespace GhostCodeBackend.PostManagement.Services;

public class CommentService : ICommentService
{
    private readonly IPostsRepository _postsRepository;
    private readonly ICacheService _cache;

    public CommentService(IPostsRepository postsRepository, ICacheService cache)
    {
        _postsRepository = postsRepository;
        _cache = cache;
    }


    public async Task<bool> WriteComment(string postid, Comment comment, CancellationToken ct = default)
    {
        return await _postsRepository.AddCommentToPostAsync(postid, comment, ct);
    }

    public async Task<(bool result, List<Comment> comments)> GetCommentsAsync(string postId, int count, CancellationToken ct = default)
    {
        
        if(count <= 0 || count > 30) return (false, null);
        
        var  result = await _postsRepository.GetHotPostCommentsAsync(postId, count, ct);
        
        return result;
    }

    public async Task<(bool result, List<Comment> comments)> GetCommentsByChunkAsync(string postId, int chunkIndex,
        CancellationToken ct = default)
    {
        return await _postsRepository.GetCommentChunk(postId, chunkIndex);
    }
}