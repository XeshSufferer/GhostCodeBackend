using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Services;

public interface ICommentService
{
    Task<Result> WriteComment(string postId, Comment comment, CancellationToken ct = default);

    Task<Result<List<Comment>>> GetCommentsAsync(string postId, int count, CancellationToken ct = default);

    Task<Result<List<Comment>>> GetCommentsByChunkAsync(string postId, int chunkIndex, CancellationToken ct = default);
}