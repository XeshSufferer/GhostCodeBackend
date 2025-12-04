using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.PostManagement.Repositories;

public class PostsRepository : IPostsRepository
{
    private readonly IMongoCollection<Post> _posts;
    private readonly IMongoCollection<LikeChunk> _coldLikes;
    private readonly IMongoCollection<CommentsChunk> _coldComments;
    private readonly ILogger<PostsRepository> _logger;

    public PostsRepository(IMongoDatabase db, ILogger<PostsRepository> logger)
    {
        _logger = logger;
        _posts = db.GetCollection<Post>("posts");
        _coldLikes = db.GetCollection<LikeChunk>("cold_likes");
        _coldComments = db.GetCollection<CommentsChunk>("cold_comments");

        // Индексы (можно вынести в миграции, но оставим тут)
        _coldLikes.Indexes.CreateOne(
            Builders<LikeChunk>.IndexKeys.Ascending(x => x.PostId), 
            new CreateIndexOptions { Unique = false });

        _coldComments.Indexes.CreateOne(
            Builders<CommentsChunk>.IndexKeys.Ascending(x => x.PostId), 
            new CreateIndexOptions { Unique = false });
    }

    // --- POSTS ---
    public async Task<Result<Post>> InsertPostAsync(Post post, CancellationToken ct = default)
    {
        try
        {
            await _posts.InsertOneAsync(post, cancellationToken: ct);
            return Result<Post>.Success(post);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to insert post");
            return Result<Post>.Failure(e.Message);
        }
    }

    public async Task<Result<Post?>> GetPostByIdAsync(string postId, CancellationToken ct = default)
    {
        try
        {
            var post = await _posts.Find(p => p.Id == postId).FirstOrDefaultAsync(ct);
            return Result<Post?>.Success(post);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get post by ID: {PostId}", postId);
            return Result<Post?>.Failure(e.Message);
        }
    }

    public async Task<Result> ReplacePostAsync(string postId, Post post, CancellationToken ct = default)
    {
        try
        {
            var result = await _posts.ReplaceOneAsync(p => p.Id == postId, post, cancellationToken: ct);
            return result.ModifiedCount > 0 
                ? Result.Success() 
                : Result.Failure("Post not found");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to replace post: {PostId}", postId);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result<List<Post>>> GetPostsPagedAsync(int skip, int limit, CancellationToken ct = default)
    {
        try
        {
            var posts = await _posts
                .Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync(ct);
            return Result<List<Post>>.Success(posts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get paged posts");
            return Result<List<Post>>.Failure(e.Message);
        }
    }

    // --- COLD COMMENTS ---
    public async Task<Result> InsertCommentsChunkAsync(CommentsChunk chunk, CancellationToken ct = default)
    {
        try
        {
            await _coldComments.InsertOneAsync(chunk, ct);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to insert comments chunk");
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result<CommentsChunk?>> GetCommentsChunkAsync(string postId, int chunkIndex, CancellationToken ct = default)
    {
        try
        {
            var chunk = await _coldComments
                .Find(c => c.PostId == postId && c.ChunkIndex == chunkIndex)
                .FirstOrDefaultAsync(ct);
            return Result<CommentsChunk?>.Success(chunk);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get comments chunk for post {PostId}, chunk {Index}", postId, chunkIndex);
            return Result<CommentsChunk?>.Failure(e.Message);
        }
    }

    // --- COLD LIKES ---
    public async Task<Result> InsertLikesChunkAsync(LikeChunk chunk, CancellationToken ct = default)
    {
        try
        {
            await _coldLikes.InsertOneAsync(chunk, ct);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to insert likes chunk");
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result<List<LikeChunk>>> GetLikesChunksByPostIdAsync(string postId, CancellationToken ct = default)
    {
        try
        {
            var chunks = await _coldLikes
                .Find(l => l.PostId == postId)
                .ToListAsync(ct);
            return Result<List<LikeChunk>>.Success(chunks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get likes chunks for post {PostId}", postId);
            return Result<List<LikeChunk>>.Failure(e.Message);
        }
    }
}