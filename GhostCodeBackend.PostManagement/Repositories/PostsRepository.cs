using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

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
}