using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Repositories;

public interface IPostsRepository
{
    Task<Result<Post>> InsertPostAsync(Post post, CancellationToken ct = default);
    Task<Result<Post?>> GetPostByIdAsync(string postId, CancellationToken ct = default);
    Task<Result> ReplacePostAsync(string postId, Post post, CancellationToken ct = default);
    Task<Result<List<Post>>> GetPostsPagedAsync(int skip, int limit, CancellationToken ct = default);
    Task<Result> InsertCommentsChunkAsync(CommentsChunk chunk, CancellationToken ct = default);
    Task<Result<CommentsChunk?>> GetCommentsChunkAsync(string postId, int chunkIndex, CancellationToken ct = default);
    Task<Result> InsertLikesChunkAsync(LikeChunk chunk, CancellationToken ct = default);
    Task<Result<List<LikeChunk>>> GetLikesChunksByPostIdAsync(string postId, CancellationToken ct = default);
}