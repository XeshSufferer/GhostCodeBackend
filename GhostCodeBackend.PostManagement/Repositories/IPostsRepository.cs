using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Repositories;

public interface IPostsRepository
{
    Task<(bool result, List<Post>? posts)> GetLastPostsAsync(int count, CancellationToken ct = default);
    Task<(bool result, Post? post)> PostAsync(Post post, CancellationToken ct = default);

    Task<(bool result, List<Comment>? comments)> GetHotPostCommentsAsync(string postId, int count,
        CancellationToken ct = default);

    Task<bool> AddCommentToPostAsync(string postid, Comment comment, CancellationToken ct = default);
    Task<bool> UpdatePostAsync(string postid, Post post, CancellationToken ct = default);
    Task<(bool result, Post? post)> GetPostAsync(string postid, CancellationToken ct = default);
}