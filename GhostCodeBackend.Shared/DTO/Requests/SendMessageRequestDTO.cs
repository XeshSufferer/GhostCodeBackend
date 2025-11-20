namespace GhostCodeBackend.Shared.DTO.Requests;

public class SendMessageRequestDTO
{
    public string ChatId { get; set; }
    public string ReplyTo { get; set; }
    public string Message { get; set; }
}