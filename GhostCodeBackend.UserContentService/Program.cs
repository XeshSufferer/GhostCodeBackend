using System.Security.Claims;
using System.Text;
using GhostCodeBackend.Shared.Сache;
using GhostCodeBackend.UserContentService.Helpers;
using GhostCodeBackend.UserContentService.Repositories;
using GhostCodeBackend.UserContentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Minio.DataModel.Args;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shared.CfgObjects;


const long MAX_CONTENT_SIZE = 10L /* cuz 10 MB */ * 1024 * 1024;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

JwtOptions jwtOpt = 
    new JwtOptions()
    {
        Audience = cfg["JWTAudience"],
        Issuer = cfg["JWTIssuer"],
        Key = cfg["JWTKey"],
        ExpireMinutes = int.Parse(cfg["JWTExpireMinutes"])
    };

builder.AddRedisDistributedCache("redis");

builder.AddDefaultAuthorization(builder.Configuration);



builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetConnectionString("mongodb")));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("main");
});

builder.Services.AddSingleton<IImageCompresser, ImageCompresser>();
builder.Services.AddSingleton<ILiteUserRepository, LiteUserRepository>();
builder.Services.AddSingleton<IStorage, Storage>();
builder.Services.AddScoped<ICustomizerService, CustomizerService>();
builder.Services.AddSingleton<ICacheService, RedisService>();

builder.WebHost.ConfigureKestrel(o =>
    o.Limits.MaxRequestBodySize = MAX_CONTENT_SIZE); 

builder.AddServiceDefaults();


builder.AddMinioClient("minio");

builder.AddDefaultCors();
builder.AddDefaultRateLimits(3, 30);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseDefaultRateLimits();
app.UseDefaultCors();

app.UseSwagger();
app.UseSwaggerUI();

var minio = app.Services.GetRequiredService<IMinioClient>();
var buckets = new[] { "avatars", "headers" };
foreach (var b in buckets)
{
    try
    {
        var exist = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(b));
        if (!exist)
            await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(b));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MinIO bucket {b}: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.MapGet("link/{bucket}/{*key}", async (
    string bucket, string key,
    HttpContext context,   
    IStorage storage) =>
{
    await storage.Get(context, bucket.ToLower(), key.ToLower());
});

app.MapPost("/uploadHeader", async (IFormFile file, ICustomizerService customizer, ClaimsPrincipal user) =>
{
    var result = await customizer.SetHeader(file, user.Identity.Name);
    return result.result ? Results.Ok(new { link = result.filename }) : Results.BadRequest("Error uploading header");
}).DisableAntiforgery().RequireAuthorization().RequireRateLimiting("per-ip");

app.MapPost("/uploadAvatar", async (IFormFile file, ICustomizerService customizer, ClaimsPrincipal user) =>
{
    var result = await customizer.SetAvatar(file, user.Identity.Name);
    return result.result ? Results.Ok(new { link = result.filename }) : Results.BadRequest("Error uploading avatar");
}).DisableAntiforgery().RequireAuthorization().RequireRateLimiting("per-ip");

app.Run();