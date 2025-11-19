using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;
using GhostCodeBackend.Shared.Helpers;
using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.Shared.Models;

[Table("chat_messages")]
public class MessageRecord
{
    [PartitionKey(0)]
    public string ChatId { get; set; } = string.Empty;

    [PartitionKey(1)]
    public int WeekBucket { get; set; } // например: 2025047

    [ClusteringKey(0, SortOrder.Descending)]
    public string Id { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ReplyToId { get; set; }
    public List<Reaction>? Reactions { get; set; }
    public List<string>? PictureLinks { get; set; }
    public DateTime CreatedAt { get; set; }

    // Преобразование в общий Message
    public Message ToMessage() => new()
    {
        Id = Id,
        ChatId = ChatId,
        SenderId = SenderId,
        Text = Text,
        ReplyToId = ReplyToId,
        Reactions = Reactions ?? new(),
        PictureLinks = PictureLinks ?? new(),
        CreatedAt = CreatedAt
    };

    // Создание из Message
    public static MessageRecord FromMessage(Message message)
    {
        return new()
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Text = message.Text,
            ReplyToId = message.ReplyToId,
            Reactions = message.Reactions,
            PictureLinks = message.PictureLinks,
            CreatedAt = message.CreatedAt,
            WeekBucket = WeekBucketHelper.GetWeekBucket(message.CreatedAt)
        };
    }
}