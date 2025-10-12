using GhostCodeBackend.Shared.Сache;
using GhostCodeBackend.UserContentService.Repositories;
using Minio;
using Minio.DataModel.Args;

namespace GhostCodeBackend.UserContentService.Services;

public class CustomizerService : ICustomizerService
{
    
    private readonly string[] _imageExtensions =
    {
        ".png",
        ".jpg",
        ".jpeg"
    };
    
    private readonly ILiteUserRepository _userRepository;
    private readonly IStorage _storage;
    private readonly ICacheService _cache;

    public CustomizerService(ILiteUserRepository userRepository, IStorage storage, ICacheService cache)
    {
        _userRepository = userRepository;
        _storage = storage;
        _cache = cache;
    }

    public async Task<(bool result, string filename)> SetHeader(IFormFile file, string userid)
    {
        
        if (file == null)
        {
            return (false, "");
        }
        
        if (file.Length <= 0)
        {
            return (false, "");
        }
        
        string extension = Path.GetExtension(file.FileName);
        
        if (!_imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return (false, "");
        }
        
        var userGettingResult = await _userRepository.GetUser(userid);
        if (userGettingResult == null)
        {
            return (false, "");
        }
        
        var result = await _storage.Upload(file, "headers");
        if (!result.result)
        {
            return (false, "");
        }
        
        userGettingResult.HeaderLink = result.imageid;
        var userUpdateResult = await _userRepository.UpdateUser(userGettingResult);
        if (!userUpdateResult.result)
        {
            return (false, "");
        }

        if (await _cache.ExistsAsync($"accountManagement:userdata:{userid}"))
        {
            await _cache.RemoveAsync($"accountManagement:userdata:{userid}");
        }
        
        bool finalResult = result.result && userUpdateResult.result && userGettingResult != null;
        return (finalResult, result.imageid);
    }

    public async Task<(bool result, string filename)> SetAvatar(IFormFile file, string userid)
    {
        if (file.Length <= 0)
            return (false, "");
    
    
        string extension = Path.GetExtension(file.FileName);
        
        
        if (!_imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return (false, "");

        var userGettingResult = await _userRepository.GetUser(userid);
        var result = await _storage.Upload(file, "avatars");
        userGettingResult.AvatarLink = result.imageid;
        var userUpdateResult = await _userRepository.UpdateUser(userGettingResult);
        
        if (await _cache.ExistsAsync($"accountManagement:userdata:{userid}"))
        {
            await _cache.RemoveAsync($"accountManagement:userdata:{userid}");
        }
        
        return (result.result && userUpdateResult.result && userGettingResult != null, result.imageid);
    }
}