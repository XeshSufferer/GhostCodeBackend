using System.Security.Claims;
using System.Text;
using GhostCodeBackend.UserContentService.Repositories;
using GhostCodeBackend.UserContentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Minio.DataModel.Args;
using MongoDB.Driver;
using Shared.CfgObjects;


const long MAX_CONTENT_SIZE = 10L /* cuz 10 MB */ * 1024 * 1024;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;



JwtOptions jwtOpt = 
    new JwtOptions()
    {
        Audience = cfg["JWTAudience"],
        Issuer = cfg["JWTIssuer"],
        Key = cfg["JWTKey"],
        ExpireMinutes = int.Parse(cfg["JWTExpireMinutes"])
    };

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOpt.Issuer,
            ValidAudience = jwtOpt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.Key))
        };
    });



builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetConnectionString("mongodb")));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("main");
});

builder.Services.AddSingleton<ILiteUserRepository, LiteUserRepository>();
builder.Services.AddSingleton<IStorage, Storage>();
builder.Services.AddScoped<ICustomizerService, CustomizerService>();


builder.WebHost.ConfigureKestrel(o =>
    o.Limits.MaxRequestBodySize = MAX_CONTENT_SIZE); 

builder.AddServiceDefaults();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

builder.AddMinioClient("minio");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("per-ip", config =>
    {
        config.PermitLimit   = 3;          // сколько
        config.Window        = TimeSpan.FromMinutes(1);
        config.QueueLimit    = 0;           // без очереди – сразу 429
        config.AutoReplenishment = true;
    });
    
    opt.OnRejected = (ctx, ct) =>
    {
        var ip = ctx.HttpContext.Connection.RemoteIpAddress?.ToString();
        IpBanMiddleware.Ban(ip, TimeSpan.FromMinutes(30));
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

app.UseMiddleware<IpBanMiddleware>();


app.MapDefaultEndpoints();



app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.UseRateLimiter();



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