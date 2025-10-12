namespace GhostCodeBackend.UserContentService.Repositories;

public interface IStorage
{
    Task<(bool result, string imageid)> Upload(IFormFile file, string bucket);
    Task Get(HttpContext ctx, string bucket, string key);
}