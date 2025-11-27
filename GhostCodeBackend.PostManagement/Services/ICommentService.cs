using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface ICommentService
{
    Task<Result> WriteComment(string postid, Comment comment, CancellationToken ct = default);

    Task<Result<List<Comment>>> GetCommentsAsync(string postId, int count,
        CancellationToken ct = default);
}