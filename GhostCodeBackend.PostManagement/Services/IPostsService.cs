using System.Security.Claims;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface IPostsService
{
    Task<Result<List<Post?>>> GetPosts(int skip, int limit, CancellationToken ct = default);
    Task<Result<Post>> CreatePost(PostCreationRequestDTO request, ClaimsPrincipal user);

    Task<Result<List<Comment>>> GetPostCommentsByChunk(int postId, int skip, int limit,
        CancellationToken ct = default);
    Task<Result<Post>> GetPostById(int postId, CancellationToken ct = default);
}