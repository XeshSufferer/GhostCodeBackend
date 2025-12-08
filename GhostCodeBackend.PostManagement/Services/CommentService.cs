using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.PostManagement.Services;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Сache;

public class CommentService : ICommentService
{
    private readonly IPostsRepository _postsRepository;
    private readonly ICacheService _cache;
    private readonly int _commentsLimit = 5;

    public CommentService(IPostsRepository postsRepository, ICacheService cache)
    {
        _postsRepository = postsRepository;
        _cache = cache;
    }

    public async Task<Result<List<Comment>>> GetCommentsRange(int postId, int skip, int limit)
    {
        
        if(!(await _postsRepository.PostExist(postId)).Value || postId <= 0)
            return Result<List<Comment>>.Failure("Post not exist");
        
        if(limit > _commentsLimit)
            return Result<List<Comment>>.Failure("Comments limit exceeded");
        
        return await _postsRepository.GetCommentsRangeByPostId(postId, skip, limit);
    }

    public async Task<Result> ChangeComment(string userId, int commentId, Comment comment)
    {
        var commentGettingResult = await _postsRepository.GetCommentById(commentId);

        if (userId != commentGettingResult.Value.AuthorId)
            return Result.Failure("Permission denied");

        return await _postsRepository.ChangeCommentById(commentId, comment);
    }

    public async Task<Result> WriteComment(int postId, Comment comment, CancellationToken ct = default)
    {
        if(!(await _postsRepository.PostExist(postId)).Value || postId <= 0)
            return Result.Failure("Post not exist");
        
        if(comment == null)
            return Result.Failure("Comment cannot be null");
        
        return await _postsRepository.AddComment(comment);
    }
}