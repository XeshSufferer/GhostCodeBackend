namespace GhostCodeBackend.UserContentService.Services;

public interface ICustomizerService
{
    Task<(bool result, string filename)> SetHeader(IFormFile file, string userid);
    Task<(bool result, string filename)> SetAvatar(IFormFile file, string userid);
}