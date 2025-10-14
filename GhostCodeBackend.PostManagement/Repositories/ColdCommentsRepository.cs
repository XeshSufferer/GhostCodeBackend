using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.PostManagement.Repositories;

public class ColdCommentsRepository : IColdCommentsRepository
{
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<CommentsChunk> _coldStorage;
    
    
    /*public ColdCommentsRepository(IMongoDatabase db)
    {
        _db = db;
        db.CreateCollection("cold_comments");
        _coldStorage = _db.GetCollection<CommentsChunk>("cold_comments");
    }*/

    public async Task<(bool result, List<Comment> comments)> GetOldComments(string postId, int count, CancellationToken ct = default)
    {
        try
        {
            var comments = await _coldStorage
                .Find(FilterDefinition<CommentsChunk>.Empty)
                .SortByDescending(p => p.CreatedAt)
                .Limit(1)
                .ToListAsync(ct);
            return (comments.Count > 0, comments[0].Comments);
        }catch(Exception e)
        {
            return (false, null);
        }
    }

    public async Task<bool> AddCommentsToCold(string postId, List<Comment> comments)
    {
        var chunk = new CommentsChunk()
        {
            PostId = postId,
            Comments = comments,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _coldStorage.InsertOneAsync(chunk);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}