using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.PostManagement.Services;
using GhostCodeBackend.Shared.Models;

public class LikeService : ILikeService
{
    private readonly IPostsRepository _posts;
    private readonly int _likesCacheLimit = 5;

    public LikeService(IPostsRepository posts)
    {
        _posts = posts;
    }

    public async Task<Result> Like(string postId, string userId, CancellationToken ct = default)
    {
        var postResult = await _posts.GetPostByIdAsync(postId, ct);
        if (!postResult.IsSuccess || postResult.Value == null)
            return Result.Failure("Post not found");

        var post = postResult.Value;

        // Проверяем, лайкал ли уже
        bool isLiked = post.LikerSegments.Any(s => s.Id == userId);
        bool isLikedInCold = false;

        if (!isLiked)
        {
            var coldChunks = await _posts.GetLikesChunksByPostIdAsync(postId, ct);
            if (coldChunks.IsSuccess)
            {
                isLikedInCold = coldChunks.Value.Any(chunk => chunk.Users.Contains(userId));
            }
        }

        if (isLiked || isLikedInCold)
        {
            // Удаляем лайк
            post.LikerSegments.RemoveAll(s => s.Id == userId);
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
        }
        else
        {
            // Добавляем лайк
            post.LikerSegments.Add(new LikeSegment { Id = userId, CreatedAt = DateTime.UtcNow });
            post.LikesCount++;
        }

        // Если сегментов стало больше лимита — сливаем в cold storage
        if (post.LikerSegments.Count > _likesCacheLimit)
        {
            var users = post.LikerSegments.Select(s => s.Id).ToArray();
            var chunk = new LikeChunk
            {
                PostId = postId,
                ChunkIndex = post.LikesLastChunkIndex,
                Users = users
            };

            var insertResult = await _posts.InsertLikesChunkAsync(chunk, ct);
            if (insertResult.IsSuccess)
            {
                post.LikesLastChunkIndex++;
                post.LikerSegments.Clear();
            }
            else
            {
                return Result.Failure("Failed to archive likes to cold storage");
            }
        }

        var updateResult = await _posts.ReplacePostAsync(postId, post, ct);
        return updateResult;
    }
}