using System.Security.Claims;
using System.Text;
using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.PostManagement.Services;
using GhostCodeBackend.Shared.DTO.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shared.CfgObjects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();



var cfg = builder.Configuration;

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(cfg.GetConnectionString("mongodb")));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("main");
});

builder.Services.AddSingleton<IPostsRepository, PostsRepository>();
builder.Services.AddScoped<IPostsService, PostsService>();


JwtOptions jwtOpt = 
    new JwtOptions()
    {
        Audience = cfg["JWTAudience"],
        Issuer = cfg["JWTIssuer"],
        Key = cfg["JWTKey"],
        ExpireMinutes = int.Parse(cfg["JWTExpireMinutes"])
    };


builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.AddServiceDefaults();
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

builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("per-ip", config =>
    {
        config.PermitLimit   = 10;          // сколько
        config.Window        = TimeSpan.FromMinutes(1);
        config.QueueLimit    = 0;           // без очереди – сразу 429
        config.AutoReplenishment = true;
    });
    
    opt.OnRejected = (ctx, ct) =>
    {
        var ip = ctx.HttpContext.Connection.RemoteIpAddress?.ToString();
        IpBanMiddleware.Ban(ip, TimeSpan.FromMinutes(5));
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };
});

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

var app = builder.Build();


app.UseMiddleware<IpBanMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapPost("/create", async (PostCreationRequestDTO req, IPostsService posts, ClaimsPrincipal user) =>
{
    var result = await posts.CreatePost(req, user);
    return result.result ? Results.Ok(result.post) : Results.BadRequest("Post creation Failed");
}).RequireAuthorization().RequireRateLimiting("per-ip");

app.MapGet("/getPosts/{count}", async (int count, IPostsService posts) =>
{
    var result = await posts.GetPosts(count);
    return result.result ? Results.Ok(new { posts = result.posts }) : Results.BadRequest("Post get Failed");
}).RequireAuthorization().RequireRateLimiting("per-ip");

app.Run();