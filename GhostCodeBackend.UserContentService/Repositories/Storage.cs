using Minio;
using Minio.DataModel.Args;

namespace GhostCodeBackend.UserContentService.Repositories;

public class Storage : IStorage
{

    private readonly IMinioClient _minio;

    public Storage(IMinioClient minio)
    {
        _minio = minio;
    }
    
    
    public async Task<(bool result, string imageid)> Upload(IFormFile file, string bucket)
    {
        Console.WriteLine($"=== Storage.Upload START ===");
        Console.WriteLine($"Bucket: {bucket}");
        Console.WriteLine($"File Name: {file.FileName}");
        Console.WriteLine($"File Size: {file.Length}");
        Console.WriteLine($"Content Type: {file.ContentType}");

        try
        {
            string imageId = Guid.NewGuid().ToString();
            Console.WriteLine($"Generated ImageID: {imageId}");

            await using var stream = file.OpenReadStream();
            Console.WriteLine($"Stream opened successfully. Length: {stream.Length}");

            var put = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(imageId)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(file.ContentType);

            Console.WriteLine($"Attempting to upload to MinIO...");
            Console.WriteLine($"Bucket: {bucket}");
            Console.WriteLine($"Object: {imageId}");
            Console.WriteLine($"Object Size: {stream.Length}");

            await _minio.PutObjectAsync(put);
        
            Console.WriteLine($"✅ MinIO upload successful");
            Console.WriteLine($"=== Storage.Upload END ===\n");
        
            return (true, imageId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"❌ MinIO UPLOAD ERROR: {e.Message}");
            Console.WriteLine($"Stack Trace: {e.StackTrace}");
        
            if (e.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
            }
        
            Console.WriteLine($"=== Storage.Upload END ===\n");
            return (false, "");
        }
    }

    public async Task Get(HttpContext ctx, string bucket, string key)
    {
        ctx.Response.ContentType = "application/octet-stream";

        await _minio.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithCallbackStream(async stream =>
                {
                    await stream.CopyToAsync(ctx.Response.Body);
                }));
    }
}