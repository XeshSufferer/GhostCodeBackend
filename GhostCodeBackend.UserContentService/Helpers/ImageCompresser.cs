using GhostCodeBackend.UserContentService.OptionsObj;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace GhostCodeBackend.UserContentService.Helpers;

public class ImageCompresser : IImageCompresser
{
    
    
    public async Task<MemoryStream> Compress(IFormFile file, CompressConfig cfg)
    {

        int quality = 100;
        
        await using var inStream = file.OpenReadStream();
        using var img = await Image.LoadAsync(inStream);

        img.Mutate(i => i.Resize(new ResizeOptions(){Mode = ResizeMode.Max,
            Size = new Size(cfg.MaxW, cfg.MaxH)}));
        
        MemoryStream outStream = new();
        while (quality >= 40)
        {
            outStream.SetLength(0);
            await img.SaveAsync(outStream, new JpegEncoder{Quality = quality});
            if (outStream.Length <= cfg.TargetFileSize) break;
            quality -= 5;
        }
        outStream.Position = 0;

        return outStream;
    }
}