namespace GhostCodeBackend.Shared.DTO.Requests;

public class GitRepositoryCreateRequestDTO
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string RepositoryName { get; set; }
    public bool IsPublic { get; set; }
}