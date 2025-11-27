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


    public async Task<Result> WriteComment(string postid, Comment comment, CancellationToken ct = default)
    {
        return await _postsRepository.AddCommentToPostAsync(postid, comment, ct);
    }

    public async Task<Result<List<Comment>>> GetCommentsAsync(string postId, int count, CancellationToken ct = default)
    {
        
        if(count <= 0 || count > 30) return Result<List<Comment>>.Failure("Invalid comment count");
        
        var  result = await _postsRepository.GetHotPostCommentsAsync(postId, count, ct);
        
        return result;
    }

    public async Task<Result<List<Comment>>> GetCommentsByChunkAsync(string postId, int chunkIndex,
        CancellationToken ct = default)
    {
        return Result<List<Comment>>.Success((await _postsRepository.GetCommentChunk(postId, chunkIndex)).Value.Comments);
    }
}