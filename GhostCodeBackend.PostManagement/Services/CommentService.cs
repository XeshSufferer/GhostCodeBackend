using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.PostManagement.Services;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Сache;

public class CommentService : ICommentService
{
    private readonly IPostsRepository _postsRepository;
    private readonly ICacheService _cache;
    private readonly int _commentsCacheLimit = 5;

    public CommentService(IPostsRepository postsRepository, ICacheService cache)
    {
        _postsRepository = postsRepository;
        _cache = cache;
    }

    public async Task<Result> WriteComment(string postId, Comment comment, CancellationToken ct = default)
    {
        var postResult = await _postsRepository.GetPostByIdAsync(postId, ct);
        if (!postResult.IsSuccess || postResult.Value == null)
            return Result.Failure("Post not found");

        var post = postResult.Value;
        post.Comments.Add(comment);
        post.CommentsCount++;

        // Если превышен лимит — архивируем
        if (post.Comments.Count > _commentsCacheLimit)
        {
            var chunk = new CommentsChunk
            {
                PostId = postId,
                ChunkIndex = post.CommentsLastChunkIndex,
                Comments = new List<Comment>(post.Comments),
                CreatedAt = DateTime.UtcNow
            };

            var insertResult = await _postsRepository.InsertCommentsChunkAsync(chunk, ct);
            if (insertResult.IsSuccess)
            {
                post.CommentsLastChunkIndex++;
                post.Comments.Clear();
            }
            else
            {
                return Result.Failure("Failed to archive comments to cold storage");
            }
        }

        return await _postsRepository.ReplacePostAsync(postId, post, ct);
    }

    public async Task<Result<List<Comment>>> GetCommentsAsync(string postId, int count, CancellationToken ct = default)
    {
        if (count <= 0 || count > 30)
            return Result<List<Comment>>.Failure("Comment count must be between 1 and 30");

        var postResult = await _postsRepository.GetPostByIdAsync(postId, ct);
        if (!postResult.IsSuccess || postResult.Value == null)
            return Result<List<Comment>>.Failure("Post not found");

        var hotComments = postResult.Value.Comments.Take(count).ToList();
        return Result<List<Comment>>.Success(hotComments);
    }

    public async Task<Result<List<Comment>>> GetCommentsByChunkAsync(string postId, int chunkIndex, CancellationToken ct = default)
    {
        var chunkResult = await _postsRepository.GetCommentsChunkAsync(postId, chunkIndex, ct);
        if (!chunkResult.IsSuccess)
            return Result<List<Comment>>.Failure(chunkResult.Error);
        if (chunkResult.Value == null)
            return Result<List<Comment>>.Failure("Chunk not found");

        return Result<List<Comment>>.Success(chunkResult.Value.Comments);
    }
}