using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Models.Enums;

namespace GhostCodeBackend.UserContentService.Services;

public interface IFileStorageService
{
    Task<Result<string>> UploadFile(IFormFile file, FileQuality quality, string bucket);
}