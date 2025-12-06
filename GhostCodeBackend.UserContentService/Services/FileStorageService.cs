using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Models.Enums;
using GhostCodeBackend.Shared.Сache;
using GhostCodeBackend.UserContentService.Helpers;
using GhostCodeBackend.UserContentService.OptionsObj;
using GhostCodeBackend.UserContentService.Repositories;

namespace GhostCodeBackend.UserContentService.Services;

public class FileStorageService : IFileStorageService
{
    
    private readonly string[] _imageExtensions =
    {
        ".png",
        ".jpg",
        ".jpeg"
    };
    
    private readonly IStorage _storage;
    private readonly ICacheService _cache;
    private readonly IImageCompresser _compresser;

    public FileStorageService(IStorage storage, ICacheService cache, IImageCompresser compresser)
    {
        _storage = storage;
        _cache = cache;
        _compresser = compresser;
    }

    public async Task<Result<string>> UploadFile(IFormFile file, FileQuality quality, string bucket)
    {
        switch (quality)
        {
            case FileQuality.Min:
            {
                
                var validateResult = await IFormFileIsValid(file);
                
                if(!validateResult.IsSuccess)
                    return Result<string>.Failure(validateResult.Error);

                var compressCfg = new CompressConfig
                {
                    MaxH = 512,
                    MaxW = 512,
                    TargetFileSize = 80_000
                };
                
                return await UploadFile__Internal(file, compressCfg, bucket); 
                break;
            }
            
            case FileQuality.Normal:
            {
                var validateResult = await IFormFileIsValid(file);
                
                if(!validateResult.IsSuccess)
                    return Result<string>.Failure(validateResult.Error);

                var compressCfg = new CompressConfig
                {
                    MaxH = 1920,
                    MaxW = 1920,
                    TargetFileSize = 300_000
                };
                
                return await UploadFile__Internal(file, compressCfg, bucket);
                break;
            }

            case FileQuality.Source:
            {
                var validateResult = await IFormFileIsValid(file);
                
                if(!validateResult.IsSuccess)
                    return Result<string>.Failure(validateResult.Error);

                var compressCfg = new CompressConfig
                {
                    MaxH = 3840,
                    MaxW = 3840,
                    TargetFileSize = 2_000_000
                };
                
                return await UploadFile__Internal(file, compressCfg, bucket);
                break;
            }
        }

        throw new ArgumentException("Invalid quality format");
    }

    private async Task<Result> IFormFileIsValid(IFormFile file)
    {
        if (file == null)
        {
            return Result.Failure("File is null");
        }
        
        if (file.Length <= 0)
        {
            return Result.Failure("File is empty");
        }
        
        string extension = Path.GetExtension(file.FileName);
        
        if (!_imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure($"Invalid file format ({extension})");
        }
        
        return Result.Success();
    }

    private async Task<Result<string>> UploadFile__Internal(IFormFile file, CompressConfig compressConfig, string bucket)
    {
        var compressedImage = await _compresser.Compress(file, compressConfig);
        var result = await _storage.Upload(compressedImage, bucket);
        if (!result.result)
        {
            return Result<string>.Failure("File upload failed");
        }
        
        return Result<string>.Success(result.imageid);
    }
}