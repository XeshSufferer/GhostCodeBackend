using System.Runtime.CompilerServices;
using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.PostManagement.Repositories;

public class ColdLikesRepository : IColdLikesRepository
{
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<LikeChunk> _coldStorage;

    
    private readonly IPostsRepository _posts;
    
    public ColdLikesRepository(IMongoDatabase db, IPostsRepository posts)
    {
        _posts = posts;
        _db = db;
        db.CreateCollection("cold_likes");
        _coldStorage = _db.GetCollection<LikeChunk>("cold_likes");
    }

    public async Task<bool> Like(string postid, string likerid, CancellationToken ct = default)
    {

        LikeSegment segment = new LikeSegment()
        {
            Id = likerid,
            CreatedAt = DateTime.UtcNow,
        };
        
        var post = await _posts.GetPostAsync(postid, ct);

        if (!post.result) return false;

        if (post.post.LikerSegments.AsQueryable().Where(s => s.Id == likerid).Any())
        {
            
            post.post.LikerSegments.Remove(segment);
            post.post.LikesCount--;
        }
        else
        {
            post.post.LikerSegments.Add(segment);
            post.post.LikesCount++;
        }
        var updResult = await _posts.UpdatePostAsync(post.post.Id, post.post, ct);
        
        return updResult;
    }
}