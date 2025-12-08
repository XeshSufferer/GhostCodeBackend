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

    public async Task<Result> Like(int postId, string userId, CancellationToken ct = default)
    {
        if(!(await _posts.PostExist(postId)).Value || postId <= 0)
            return Result.Failure("Post not exist");

        var post = await _posts.GetPostById(postId);
        
        if (!await _posts.UserIsLikedPost(userId, postId))
        {
            var like = new Like
            {
                PostId = postId,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };
            var result = await _posts.AddLike(like);
            if (result.IsSuccess)
            {
                post.Value.LikesCount++;
                await _posts.UpdatePost(post.Value);
            }
            return result;
        }
        else
        {
            var result = await _posts.DeleteLike(postId, userId);
            if (result.IsSuccess)
            {
                post.Value.LikesCount--;
                await _posts.UpdatePost(post.Value);
            }

            return result;
        }

        
    }
}