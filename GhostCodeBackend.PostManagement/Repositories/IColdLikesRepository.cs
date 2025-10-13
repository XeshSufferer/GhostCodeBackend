namespace GhostCodeBackend.PostManagement.Repositories;

public interface IColdLikesRepository
{
    Task<bool> Like(string postid, string likerid, CancellationToken ct = default);
}