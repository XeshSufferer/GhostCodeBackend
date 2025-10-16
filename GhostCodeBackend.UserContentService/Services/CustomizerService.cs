using GhostCodeBackend.Shared.Сache;
using GhostCodeBackend.UserContentService.Helpers;
using GhostCodeBackend.UserContentService.OptionsObj;
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
    private readonly IImageCompresser _compresser;

    public CustomizerService(ILiteUserRepository userRepository, IStorage storage, ICacheService cache, IImageCompresser compresser)
    {
        _userRepository = userRepository;
        _storage = storage;
        _cache = cache;
        _compresser = compresser;
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
        
        CompressConfig compressConfig =  new CompressConfig()
        {
            MaxH = 300,
            MaxW = 1200,
            TargetFileSize = 100_000
        };
        var compressedImage = await _compresser.Compress(file, compressConfig);
        var result = await _storage.Upload(compressedImage, "headers");
        if (!result.result)
        {
            return (false, "");
        }
        
        await _storage.Delete("headers", userGettingResult.HeaderLink);
        
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
        
        
        CompressConfig compressConfig =  new CompressConfig()
        {
            MaxH = 128,
            MaxW = 128,
            TargetFileSize = 40_000
        };
        var compressedImage = await _compresser.Compress(file, compressConfig);
        var result = await _storage.Upload(compressedImage, "avatars");
        userGettingResult.AvatarLink = result.imageid;
        var userUpdateResult = await _userRepository.UpdateUser(userGettingResult);
        
        await _storage.Delete("avatars", userGettingResult.HeaderLink);
        
        if (await _cache.ExistsAsync($"accountManagement:userdata:{userid}"))
        {
            await _cache.RemoveAsync($"accountManagement:userdata:{userid}");
        }
        
        return (result.result && userUpdateResult.result && userGettingResult != null, result.imageid);
    }
}