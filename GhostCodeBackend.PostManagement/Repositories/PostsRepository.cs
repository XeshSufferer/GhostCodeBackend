using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GhostCodeBackend.PostManagement.Repositories;

public class PostsRepository : IPostsRepository
{
    
    private readonly IMongoDatabase _db;

    private readonly IMongoCollection<Post> _posts;

    public PostsRepository(IMongoDatabase db)
    {
        _db = db;
        db.CreateCollection("posts");
        _posts = db.GetCollection<Post>("posts");
    }

    public async Task<(bool result, Post? post)> PostAsync(Post post, CancellationToken ct = default)
    {
        try
        {
            await _posts.InsertOneAsync(post, ct);
            return (true, post);
        }
        catch (Exception e)
        {
            return (false, null);
        }
    }

    public async Task<(bool result, List<Post>? posts)> GetLastPostsAsync(int count, CancellationToken ct = default)
    {
        try
        {
            var posts = await _posts
                .Find(FilterDefinition<Post>.Empty)
                .SortByDescending(p => p.CreatedAt)
                .Limit(count)
                .ToListAsync(ct);
            return (posts.Count == count, posts);
        }
        catch (Exception e)
        {
            return (false, null);
        }
    }

    public async Task<(bool result, List<Comment>? comments)> GetHotPostCommentsAsync(string postId, int count, CancellationToken ct = default)
    {
        try
        {
            var comments = (await _posts.AsQueryable().Where(p => p.Id == postId).FirstOrDefaultAsync(ct)).Comments.Take(count);
            
            return (comments.ToList().Count > 0, comments.ToList());
        }
        catch (Exception e)
        {
            return (false, null);
        }
    }

    public async Task<bool> AddCommentToPostAsync(string postid, Comment comment, CancellationToken ct = default)
    {
        try
        {
            var post = await _posts.AsQueryable().Where(p => p.Id == postid).FirstOrDefaultAsync();
            post.Comments.Add(comment);
            post.CommentsCount++;
            await _posts.ReplaceOneAsync(p => p.Id == postid, post);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<(bool result, Post? post)> GetPostAsync(string postid, CancellationToken ct = default)
    {
        try
        {
            var post = await _posts.AsQueryable().Where(p => p.Id == postid).FirstOrDefaultAsync();
            return (post != null, post);
        }
        catch (Exception e)
        {
            return (false, null);
        }
    }

    public async Task<bool> UpdatePostAsync(string postid, Post post, CancellationToken ct = default)
    {
        try
        {
            var res = await _posts.ReplaceOneAsync(p => p.Id == postid, post);

            return res.ModifiedCount > 0;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    
}