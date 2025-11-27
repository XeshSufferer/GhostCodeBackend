using System.Security.Claims;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface IPostsService
{
    Task<Result<List<Post?>>> GetPosts(int count, CancellationToken ct = default);
    Task<Result<Post>> CreatePost(PostCreationRequestDTO request, ClaimsPrincipal user);

    Task<Result<CommentsChunk>> GetPostCommentsByChunk(string postId, int chunkId,
        CancellationToken ct = default);
}