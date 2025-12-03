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
        if(request.Title.Length > _maxCharsInTitle ||  request.Title.Length < 1) return Result<Post>.Failure($"Title must be between 1 and {_maxCharsInTitle}");
        if(request.Body.Length > _maxCharsInBody || request.Body.Length < 1) return Result<Post>.Failure($"Body  must be between 1 and {_maxCharsInBody}");
        
        Post post = new Post
        {
            AuthorId = user.Identity.Name,
            Title = request.Title,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow,
        };
        
        var result = await _posts.PostAsync(post);

        return result.IsSuccess ? Result<Post>.Success(post) : Result<Post>.Failure("");
    }

    public async Task<Result<List<Post?>>> GetPosts(int count, CancellationToken ct = default)
    {
        if (limit <= 0)
            return Result<List<Post>>.Failure("Limit must be greater than zero.");
        if (skip < 0)
            return Result<List<Post>>.Failure("Skip must be non-negative.");
            
        if(count > _maxPostsPerRequest) return Result<List<Post?>>.Failure($"Max requested post: {_maxPostsPerRequest}");
        return await _posts.GetLastPostsAsync(count);
    }

    public async Task<Result<CommentsChunk>> GetPostCommentsByChunk(string postId, int chunkId, CancellationToken ct = default)
    {
        return await _posts.GetCommentChunk(postId, chunkId, ct);
    }
}