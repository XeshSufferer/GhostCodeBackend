using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Repositories;

public interface IPostsRepository
{
    Task<Result<bool>> PostExist(int postId);
    Task<Result<bool>> CommentExist(int commentId);
    Task<Result<Comment>> GetCommentById(int commentId);
    Task<Result<Post>> GetPostById(int id);
    
    Task<Result<List<Post>>> GetLastPostsInRange(int skip, int limit);
    
    Task<Result> CreatePost(Post post);
    
    Task<Result> UpdatePost(Post post);
    
    Task<Result<List<Comment>>> GetCommentsRangeByPostId(int postId, int skip, int limit);
    
    Task<Result> AddComment(Comment comment);
    
    Task<Result> DeleteComment(Comment comment);
    
    Task<Result> ChangeCommentById(int commentId, Comment comment);
    
    Task<bool> UserIsLikedPost(string userId, int postId);
    
    Task<Result> AddLike(Like like);
    
    Task<Result> DeleteLike(int postId, string userId);
}