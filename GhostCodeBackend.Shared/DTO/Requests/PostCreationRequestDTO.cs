namespace GhostCodeBackend.Shared.DTO.Requests;

public class PostCreationRequestDTO
{
    public string Title { get; set; }
    public string Body { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Attachments { get; set; }
}