using GhostCodeBackend.Shared.Models;
using Microsoft.AspNetCore.Connections;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GhostCodeBackend.PostManagement.Repositories;

public class PostsRepository : IPostsRepository
{
    
    private readonly IMongoDatabase _db;

    private readonly IMongoCollection<Post> _posts;
    private readonly IMongoCollection<LikeChunk> _coldLikes;
    private readonly IMongoCollection<CommentsChunk> _coldComments;


    private readonly ILogger<PostsRepository> _logger;
    
    private readonly int _likesCacheLimit = 5;
    private readonly int _commentsCacheLimit = 5;
    public PostsRepository(IMongoDatabase db, ILogger<PostsRepository> logger)
    {
        _logger = logger;
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

    public async Task<Result<Post>> PostAsync(Post post, CancellationToken ct = default)
    {
        try
        {
            await _posts.InsertOneAsync(post, ct);
            return Result<Post>.Success(post);
        }
        catch (Exception e)
        {
            return Result<Post>.Failure(e.ToString());
        }
    }

    public async Task<Result<List<Post>>> GetLastPostsAsync(int count, CancellationToken ct = default)
    {
        try
        {
            var posts = await _posts
                .Find(FilterDefinition<Post>.Empty)
                .SortByDescending(p => p.CreatedAt)
                .Limit(count)
                .ToListAsync(ct);
            return posts != null ? Result<List<Post>>.Success(posts) :  Result<List<Post>>.Failure("Posts not found");
        }
        catch (Exception e)
        {
            return Result<List<Post>>.Failure(e.ToString());
        }
    }

    public async Task<Result<List<Comment>?>> GetHotPostCommentsAsync(string postId, int count, CancellationToken ct = default)
    {
        try
        {
            var comments = (await _posts.AsQueryable().Where(p => p.Id == postId).FirstOrDefaultAsync(ct)).Comments.Take(count);
            
            return Result<List<Comment>?>.Success(comments.ToList());
        }
        catch (Exception e)
        {
            return Result<List<Comment>?>.Failure(e.ToString());
        }
    }

    public async Task<Result> AddCommentToPostAsync(string postid, Comment comment, CancellationToken ct = default)
    {
        try
        {
            var post = await _posts.AsQueryable().Where(p => p.Id == postid).FirstOrDefaultAsync();
            post.Comments.Add(comment);
            post.CommentsCount++;
            if (post.Comments.Count > _commentsCacheLimit)
            {
                var result = await AddCommentsToCold(post.Id, post.CommentsLastChunkIndex, post.Comments);
                if (result.IsSuccess)
                {
                    post.CommentsLastChunkIndex++;
                    post.Comments.Clear();
                }
            }

            var replaceResult = await _posts.ReplaceOneAsync(p => p.Id == postid, post);
            return replaceResult.ModifiedCount > 0 ? Result.Success() : Result.Failure("Post not found");
        }
        catch (NullReferenceException nullReferenceException)
        {
            return Result.Failure("Post not found");
        }
        catch (Exception e)
        {
            return Result.Failure(e.ToString());
        }
    }
    

    public async Task<Result<Post>> GetPostAsync(string postid, CancellationToken ct = default)
    {
        try
        {
            var post = await _posts.AsQueryable().Where(p => p.Id == postid).FirstOrDefaultAsync();
            return post != null ? Result<Post>.Success(post) : Result<Post>.Failure("Post not found");
        }
        catch (Exception e)
        {
            return Result<Post>.Failure(e.ToString());
        }
    }

    public async Task<Result> UpdatePostAsync(string postid, Post post, CancellationToken ct = default)
    {
        try
        {
            var res = await _posts.ReplaceOneAsync(p => p.Id == postid, post);

            return res.ModifiedCount > 0 ?  Result.Success() : Result.Failure("Post not found");
        }
        catch (Exception e)
        {
            return Result.Failure(e.ToString());
        }
    }
    
    public async Task<Result<CommentsChunk>> GetCommentChunk(string postId, int chunkIndex, CancellationToken ct = default)
    {
        try
        {
            var comments = await _coldComments.AsQueryable().Where(c => c.PostId == postId && c.ChunkIndex == chunkIndex).ToListAsync(ct);
            return comments != null ? Result<CommentsChunk>.Success(comments[0]) : Result<CommentsChunk>.Failure("Chunk not found");
        }catch(Exception e)
        {
            return Result<CommentsChunk>.Failure(e.ToString());
        }
    }

    public async Task<Result> AddCommentsToCold(string postId, int lastChunkIndex, List<Comment> comments)
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
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return Result.Failure(e.ToString());
        }
    }
    
    public async Task<Result> Like(string postid, string likerid, CancellationToken ct = default)
    {

        LikeSegment segment = new LikeSegment()
        {
            Id = likerid,
            CreatedAt = DateTime.UtcNow,
        };
        
        var post = await GetPostAsync(postid, ct);

        if (!post.IsSuccess) return Result.Failure(post.Error);

        if ((await PostIsLikedByUser(postid, likerid, ct)).IsSuccess)
        {
            
            bool result = post.Value.LikerSegments.Remove(segment);
            if (result)
            {
                for (int i = 0; i != post.Value.LikesLastChunkIndex; i++)
                {
                    var chunk = await _coldLikes.AsQueryable().Where(c => c.PostId == postid && c.ChunkIndex == i).FirstOrDefaultAsync(ct);

                    if (chunk.Users.ToList().Remove(likerid))
                    {
                        post.Value.LikesCount--;
                        var opts = new ReplaceOptions { IsUpsert = false };
                        await _coldLikes.ReplaceOneAsync(c => c.Id == chunk.Id, chunk, opts, ct);
                    }
                }
            }
        }
        else
        {
            post.Value.LikerSegments.Add(segment);
            post.Value.LikesCount++;
        }

        if (post.Value.LikerSegments.Count > _likesCacheLimit)
        {
            var result = await AddLikesToCold(post.Value.Id, post.Value.LikesLastChunkIndex, post.Value.LikerSegments);

            if (result.IsSuccess)
            {
                post.Value.LikesLastChunkIndex++;
                post.Value.LikerSegments.Clear();
            }
        }
        
        var updResult = await UpdatePostAsync(post.Value.Id, post.Value, ct);
        
        return updResult;
    }

    public async Task<Result> AddLikesToCold(string postId, int lastChunkIndex, List<LikeSegment> segments)
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
            return Result.Failure(e.ToString());
        }
        return Result.Success();
    }

    public async Task<Result<bool>> PostIsLikedByUser(string postId, string userId, CancellationToken ct = default)
    {
        
        var postGettingResult = await GetPostAsync(postId, ct);
        
        if(!postGettingResult.IsSuccess) return Result<bool>.Failure(postGettingResult.Error);
        
        var result = postGettingResult.Value.LikerSegments.AsQueryable().Where(p => p.Id == userId).Any();
        
        if(result) return Result<bool>.Success(true);

        for (int i = 0; i != postGettingResult.Value.LikesLastChunkIndex; i++)
        {
            var chunk = await _coldLikes.AsQueryable().Where(c => c.PostId == postId && c.ChunkIndex == i).FirstOrDefaultAsync(ct);

            if (chunk.Users.Contains(userId)) return Result<bool>.Success(true);
        }

        return Result<bool>.Success(false);
    }
    
    
}