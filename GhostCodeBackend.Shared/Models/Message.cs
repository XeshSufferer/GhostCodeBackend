namespace GhostCodeBackend.Shared.Models;

public class Message
{
    public string Id { get; set; }
    public string SenderId { get; set; }
    public string ChatId { get; set; }
    public string Text { get; set; }
    public string ReplyToId { get; set; }
    public List<Reaction> Reactions { get; set; }
    public List<string> PictureLinks { get; set; }
    public DateTime CreatedAt { get; set; }
}