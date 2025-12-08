using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostCodeBackend.Shared.Models;

public class Post
{
    public int Id { get; set; } 
    public string AuthorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    [JsonIgnore]
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    
    
    // Metadata
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
}