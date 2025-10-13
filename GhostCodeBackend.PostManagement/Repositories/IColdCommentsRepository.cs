using GhostCodeBackend.Shared.Models;

namespace GhostCodeBackend.PostManagement.Repositories;

public interface IColdCommentsRepository
{
    Task<bool> AddCommentsToCold(string postId, List<Comment> comments);

    Task<(bool result, List<Comment> comments)>
        GetOldComments(string postId, int count, CancellationToken ct = default);
}