using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.Shared.Models;
using MongoDB.Driver;

namespace GhostCodeBackend.PostManagement.Services;

public class CommentService : ICommentService
{
    private readonly IColdCommentsRepository _coldCommentsRepository;
    private readonly IPostsRepository _postsRepository;

    public CommentService(IPostsRepository postsRepository, IColdCommentsRepository coldCommentsRepository)
    {
        _postsRepository = postsRepository;
        _coldCommentsRepository = coldCommentsRepository;
    }


    public async Task<bool> WriteComment(string userid, Comment comment, CancellationToken ct = default)
    {
        return await _postsRepository.AddCommentToPostAsync(userid, comment, ct);
    }

    public async Task<(bool result, List<Comment> comments)> GetCommentsAsync(string postId, int count, CancellationToken ct = default)
    {
        
        if(count <= 0 || count > 30) return (false, null);
        
        var  result = await _postsRepository.GetHotPostCommentsAsync(postId, count, ct);
        
        return result;
    }
}