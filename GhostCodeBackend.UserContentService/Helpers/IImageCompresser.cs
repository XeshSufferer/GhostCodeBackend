using GhostCodeBackend.UserContentService.OptionsObj;

namespace GhostCodeBackend.UserContentService.Helpers;

public interface IImageCompresser
{
    Task<MemoryStream> Compress(IFormFile file, CompressConfig cfg);
}