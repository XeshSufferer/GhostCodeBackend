using GhostCodeBackend.PostManagement.Db;
using GhostCodeBackend.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GhostCodeBackend.PostManagement.Repositories;

public class PostsRepository : IPostsRepository
{
    
    private readonly PostsDbContext _db;
    private readonly ILogger<PostsRepository> _logger;

    public PostsRepository(PostsDbContext db, ILogger<PostsRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<bool>> PostExist(int postId)
    {
        try
        {
            return Result<bool>.Success(await _db.Posts.AnyAsync(p => p.Id == postId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result<bool>.Failure(e.Message);
        }
    }

    public async Task<Result<bool>> CommentExist(int commentId)
    {
        try
        {
            return Result<bool>.Success(await _db.Comments.AnyAsync(p => p.Id == commentId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result<bool>.Failure(e.Message);
        }
    }
    
    public async Task<Result<Post>> GetPostById(int id)
    {
        try
        {
            var post = await _db.Posts.Where(p => p.Id == id).SingleAsync();
            return Result<Post>.Success(post);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result<Post>.Failure(e.Message);
        }
    }

    public async Task<Result<List<Post>>> GetLastPostsInRange(int skip, int limit)
    {
        try
        {
            var posts = await _db.Posts.OrderByDescending(p => p.CreatedAt).Skip(skip).Take(limit).ToListAsync();
            return posts.Count > 0 ? Result<List<Post>>.Success(posts) : Result<List<Post>>.Failure("No posts found");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result<List<Post>>.Failure(e.Message);
        }
    }

    public async Task<Result> CreatePost(Post post)
    {
        try
        {
            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> UpdatePost(Post post)
    {
        try
        {
            _db.Posts.Update(post);
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result<List<Comment>>> GetCommentsRangeByPostId(int postId, int skip, int limit)
    {
        try
        {
            var comments = await _db.Comments.Where(p => p.PostId == postId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(limit)
                .ToListAsync();

            return Result<List<Comment>>.Success(comments);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result<List<Comment>>.Failure(e.Message);
        }
    }

    public async Task<Result> AddComment(Comment comment)
    {
        try
        {
            await _db.AddAsync(comment);
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> DeleteComment(Comment comment)
    {
        try
        {
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> ChangeCommentById(int commentId, Comment comment)
    {
        try
        {
            _db.Comments.Update(comment);
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result<Comment>> GetCommentById(int commentId)
    {
        try
        {
            var result = await _db.Comments.Where(c => c.Id == commentId).SingleOrDefaultAsync();
            return Result<Comment>.Success(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result<Comment>.Failure(e.Message);
        }
    }

    public async Task<bool> UserIsLikedPost(string userId, int postId)
    {
        try
        {
            return await _db.Likes.AnyAsync(l => l.UserId == userId && l.PostId == postId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return false;
        }
    }

    public async Task<Result> AddLike(Like like)
    {
        try
        {
            await _db.Likes.AddAsync(like);
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result.Failure(e.Message);
        }
    }

    public async Task<Result> DeleteLike(int postId, string userId)
    {
        try
        {
            var like = await _db.Likes.Where(l => l.PostId == postId && l.UserId == userId).SingleOrDefaultAsync();
            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Result.Failure(e.Message);
        }
    }
    
}