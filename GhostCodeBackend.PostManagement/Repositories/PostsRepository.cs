using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GhostCodeBackend.PostManagement.Repositories;

public class PostsRepository : IPostsRepository
{
    
    private readonly IMongoDatabase _db;

    private readonly IMongoCollection<Post> _posts;
    private readonly IMongoCollection<LikeChunk> _coldLikes;
    private readonly IMongoCollection<CommentsChunk> _coldComments;


    private readonly int _likesCacheLimit = 5;
    private readonly int _commentsCacheLimit = 5;
    public PostsRepository(IMongoDatabase db)
    {
        _db = db;
        _db.CreateCollection("posts");
        _db.CreateCollection("cold_likes");
        _db.CreateCollection("cold_comments");
        
        var indexLikes =  Builders<LikeChunk>.IndexKeys.Ascending(u => u.PostId);
        var indexLikesModel = new CreateIndexModel<LikeChunk>(indexLikes, new CreateIndexOptions{ Unique = false });
        
        var indexComments =  Builders<CommentsChunk>.IndexKeys.Ascending(c => c.PostId);
        var indexCommentsModel = new CreateIndexModel<CommentsChunk>(indexComments, new CreateIndexOptions{ Unique = false });
        
        _coldComments = _db.GetCollection<CommentsChunk>("cold_comments");
        _coldLikes =  _db.GetCollection<LikeChunk>("cold_likes");
        _posts = _db.GetCollection<Post>("posts");
        
        _coldLikes.Indexes.CreateOne(indexLikesModel);
        _coldComments.Indexes.CreateOne(indexCommentsModel);
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
            if (post.Comments.Count > _commentsCacheLimit)
            {
                var result = await AddCommentsToCold(post.Id, post.CommentsLastChunkIndex, post.Comments);
                if (result)
                {
                    post.CommentsLastChunkIndex++;
                    post.Comments.Clear();
                }
            }
            
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
    
    public async Task<(bool result, List<Comment> comments)> GetCommentChunk(string postId, int chunkIndex, CancellationToken ct = default)
    {
        try
        {
            var comments = await _coldComments.AsQueryable().Where(c => c.PostId == postId && c.ChunkIndex == chunkIndex).ToListAsync(ct);
            return (true, comments[0].Comments);
        }catch(Exception e)
        {
            return (false, null);
        }
    }

    public async Task<bool> AddCommentsToCold(string postId, int lastChunkIndex, List<Comment> comments)
    {
        var chunk = new CommentsChunk()
        {
            PostId = postId,
            Comments = comments,
            CreatedAt = DateTime.UtcNow,
            ChunkIndex = lastChunkIndex++
        };

        try
        {
            await _coldComments.InsertOneAsync(chunk);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    public async Task<bool> Like(string postid, string likerid, CancellationToken ct = default)
    {

        LikeSegment segment = new LikeSegment()
        {
            Id = likerid,
            CreatedAt = DateTime.UtcNow,
        };
        
        var post = await GetPostAsync(postid, ct);

        if (!post.result) return false;

        if (await PostIsLikedByUser(postid, likerid, ct))
        {
            
            post.post.LikerSegments.Remove(segment);
            post.post.LikesCount--;
        }
        else
        {
            post.post.LikerSegments.Add(segment);
            post.post.LikesCount++;
        }

        if (post.post.LikerSegments.Count > _likesCacheLimit)
        {
            var result = await AddLikesToCold(post.post.Id, post.post.LikesLastChunkIndex, post.post.LikerSegments);

            if (result)
            {
                post.post.LikesLastChunkIndex++;
                post.post.LikerSegments.Clear();
            }
        }
        
        var updResult = await UpdatePostAsync(post.post.Id, post.post, ct);
        
        return updResult;
    }

    public async Task<bool> AddLikesToCold(string postId, int lastChunkIndex, List<LikeSegment> segments)
    {
        
        List<string> usersIds = new List<string>();

        foreach (var segment in segments)
        {
            usersIds.Add(segment.Id);
        }


        var chunk = new LikeChunk()
        {
            PostId = postId,
            ChunkIndex = lastChunkIndex,
            Users = usersIds.ToArray()
        };
        
        try
        {
            await _coldLikes.InsertOneAsync(chunk);
        }
        catch (Exception e)
        {
            return false;
        }
        return true;
    }

    public async Task<bool> PostIsLikedByUser(string postId, string userId, CancellationToken ct = default)
    {
        
        var postGettingResult = await GetPostAsync(postId, ct);
        
        if(!postGettingResult.result) return false;
        
        var result = postGettingResult.post.LikerSegments.AsQueryable().Where(p => p.Id == userId).Any();
        
        if(result) return true;

        for (int i = 0; i != postGettingResult.post.LikesLastChunkIndex; i++)
        {
            var chunk = await _coldLikes.AsQueryable().Where(c => c.PostId == postId && c.ChunkIndex == i).FirstOrDefaultAsync(ct);

            if (chunk.Users.Contains(userId)) return true;
        }

        return false;
    }
    
    
}