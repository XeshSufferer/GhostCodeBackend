using System.Runtime.CompilerServices;
using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.PostManagement.Repositories;

public class ColdLikesRepository : IColdLikesRepository
{
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<LikeChunk> _coldStorage;

    
    private readonly IPostsRepository _posts;
    
    /*public ColdLikesRepository(IMongoDatabase db, IPostsRepository posts)
    {
        _posts = posts;
        _db = db;
        db.CreateCollection("cold_likes");
        _coldStorage = _db.GetCollection<LikeChunk>("cold_likes");
    }*/

    public async Task<bool> Like(string postid, string likerid, CancellationToken ct = default)
    {

        LikeSegment segment = new LikeSegment()
        {
            Id = likerid,
            CreatedAt = DateTime.UtcNow,
        };
        
        var post = await _posts.GetPostByIdAsync(postid, ct);

        if (!post.IsSuccess) return false;

        if (post.Value.LikerSegments.AsQueryable().Where(s => s.Id == likerid).Any())
        {
            
            post.Value.LikerSegments.Remove(segment);
            post.Value.LikesCount--;
        }
        else
        {
            post.Value.LikerSegments.Add(segment);
            post.Value.LikesCount++;
        }
        var updResult = await _posts.ReplacePostAsync(post.Value.Id, post.Value, ct);
        
        return updResult.IsSuccess;
    }
}