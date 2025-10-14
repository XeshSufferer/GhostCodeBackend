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

    public async Task<(bool result, Post? post)> CreatePost(PostCreationRequestDTO request, ClaimsPrincipal user)
    {
        if(request.Title.Length > _maxCharsInTitle ||  request.Title.Length < 1) return (false, null);
        if(request.Body.Length > _maxCharsInBody || request.Body.Length < 1) return (false, null);
        
        Post post = new Post
        {
            AuthorId = user.Identity.Name,
            Title = request.Title,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow,
        };
        
        var result = await _posts.PostAsync(post);

        return result;
    }

    public async Task<(bool result, List<Post?>? posts)> GetPosts(int count, CancellationToken ct = default)
    {
        if(count > _maxPostsPerRequest) return (false, null);
        return await _posts.GetLastPostsAsync(count);
    }

    public async Task<(bool result, List<Comment> comments)> GetPostCommentsByChunk(string postId, int chunkId, CancellationToken ct = default)
    {
        return await _posts.GetCommentChunk(postId, chunkId, ct);
    }
}