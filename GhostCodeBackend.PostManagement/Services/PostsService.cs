using System.Security.Claims;
using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public class PostsService : IPostsService
{

    private readonly int _maxPostsPerRequest = 10;
    private readonly int _maxCharsInTitle = 100;
    private readonly int _maxCharsInBody = 3000;
    private readonly IPostsRepository _posts;

    public PostsService(IPostsRepository posts)
    {
        _posts = posts;
    }

    public async Task<Result<Post>> CreatePost(PostCreationRequestDTO request, ClaimsPrincipal user)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > _maxCharsInTitle)
            return Result<Post>.Failure($"Title must be between 1 and {_maxCharsInTitle} characters.");
        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Length > _maxCharsInBody)
            return Result<Post>.Failure($"Body must be between 1 and {_maxCharsInBody} characters.");

        var post = new Post
        {
            AuthorId = user.Identity?.Name ?? throw new InvalidOperationException("User identity name is null"),
            Title = request.Title,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await _posts.InsertPostAsync(post);
        return result; // Пробрасываем ошибку из репозитория
    }

    public async Task<Result<List<Post?>>> GetPosts(int skip, int limit, CancellationToken ct = default)
    {
        if (limit <= 0)
            return Result<List<Post>>.Failure("Limit must be greater than zero.");
        if (skip < 0)
            return Result<List<Post>>.Failure("Skip must be non-negative.");
            
        if(limit > _maxPostsPerRequest) return Result<List<Post?>>.Failure($"Max requested post: {_maxPostsPerRequest}");
        return await _posts.GetPostsPagedAsync(skip, limit);
    }

    public async Task<Result<CommentsChunk>> GetPostCommentsByChunk(string postId, int chunkId, CancellationToken ct = default)
    {
        return await _posts.GetCommentsChunkAsync(postId, chunkId, ct);
    }

    public async Task<Result<Post>> GetPostById(string postId, CancellationToken ct = default)
    {
        if(postId == null)
            return Result<Post>.Failure("Post id is null");
        
        if(string.IsNullOrWhiteSpace(postId))
            return Result<Post>.Failure("Post id is or white space");
        
        return await _posts.GetPostByIdAsync(postId, ct);
    }
}