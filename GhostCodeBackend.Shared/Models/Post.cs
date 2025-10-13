using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string AuthorId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CommentsCount { get; set; }
    public long LikesCount { get; set; }
    
    public int CommentChunkedCount { get; set; }
    public int LikesChunkedCount { get; set; }
    
    public List<LikeSegment> LikerSegments { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}