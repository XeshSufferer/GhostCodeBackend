using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Repositories;

public interface IPostsRepository
{
    Task<Result<List<Post>>> GetLastPostsAsync(int count, CancellationToken ct = default);
    Task<Result<Post?>> PostAsync(Post post, CancellationToken ct = default);

    Task<Result<List<Comment>?>> GetHotPostCommentsAsync(string postId, int count,
        CancellationToken ct = default);

    Task<Result> AddCommentToPostAsync(string postid, Comment comment, CancellationToken ct = default);
    Task<Result> UpdatePostAsync(string postid, Post post, CancellationToken ct = default);
    Task<Result<Post?>> GetPostAsync(string postid, CancellationToken ct = default);

    Task<Result<CommentsChunk>> GetCommentChunk(string postId, int chunkIndex,
        CancellationToken ct = default);

    Task<Result> AddCommentsToCold(string postId, int lastChunkIndex, List<Comment> comments);
    Task<Result> Like(string postid, string likerid, CancellationToken ct = default);
    Task<Result> AddLikesToCold(string postId, int lastChunkIndex, List<LikeSegment> segments);
    Task<Result<bool>> PostIsLikedByUser(string postId, string userId, CancellationToken ct = default);
}