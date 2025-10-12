using System.Security.Claims;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface IPostsService
{
    Task<(bool result, List<Post?>? posts)> GetPosts(int count, CancellationToken ct = default);
    Task<(bool result, Post? post)> CreatePost(PostCreationRequestDTO request, ClaimsPrincipal user);

}