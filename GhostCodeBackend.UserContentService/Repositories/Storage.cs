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
    
    
    public async Task<(bool result, string imageid)> Upload(MemoryStream file, string bucket)
    {

        try
        {
            string imageId = Guid.NewGuid().ToString();

            await using var stream = file;

            var put = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(imageId)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("image/jpeg");


            await _minio.PutObjectAsync(put);
            
        
            return (true, imageId);
        }
        catch (Exception e)
        {
        
            if (e.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
            }

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

    public async Task<bool> Delete(string bucket, string key)
    {
        try
        {
            var del = new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(key);
        
            await _minio.RemoveObjectAsync(del);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}