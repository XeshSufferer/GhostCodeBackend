namespace GhostCodeBackend.Shared.Models;

public class Like
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int PostId { get; set; }
    
    public Post? Post { get; set; }
}