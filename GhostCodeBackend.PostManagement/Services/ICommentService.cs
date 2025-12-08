using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface ICommentService
{
    Task<Result> WriteComment(int postId, Comment comment, CancellationToken ct = default);

    Task<Result> ChangeComment(string userId, int commentId, Comment comment);
    Task<Result<List<Comment>>> GetCommentsRange(int postId, int skip, int limit);
}