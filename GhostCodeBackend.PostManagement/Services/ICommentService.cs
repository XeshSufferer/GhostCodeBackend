using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface ICommentService
{
    Task<bool> WriteComment(string postid, Comment comment, CancellationToken ct = default);

    Task<(bool result, List<Comment> comments)> GetCommentsAsync(string postId, int count,
        CancellationToken ct = default);
}