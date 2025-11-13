using System.Security.Claims;
using System.Text;
using GhostCodeBackend.PostManagement.Repositories;
using GhostCodeBackend.PostManagement.Services;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Сache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shared.CfgObjects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cfg = builder.Configuration;

builder.AddRedisDistributedCache("redis");

builder.Services.AddSingleton<ICacheService, RedisService>();
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(cfg.GetConnectionString("mongodb")));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("main");
});

builder.Services.AddSingleton<IPostsRepository, PostsRepository>();
builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddSingleton<IColdCommentsRepository, ColdCommentsRepository>();
builder.Services.AddSingleton<IColdLikesRepository, ColdLikesRepository>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentService, CommentService>();



builder.AddDefaultCors();
builder.AddDefaultRateLimits(20, 10);
builder.AddServiceDefaults();
builder.AddDefaultAuthorization(builder.Configuration);


var app = builder.Build();

app.UseDefaultCors();
app.UseDefaultRateLimits();
app.UseDefaultAuth();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

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

app.MapPost("/likePost", async (LikePostRequestDTO req, ClaimsPrincipal user, ILikeService likeService) =>
{
    var result = await likeService.Like(req.PostId, user.Identity.Name);
    
    return result ? Results.Ok() : Results.BadRequest("Like Failed");
}).RequireAuthorization().RequireRateLimiting("per-ip");

app.MapPost("/commentPost", async (ClaimsPrincipal user, CommentPostRequestDTO req, ICommentService commentService) =>
{
    
    var comment = new Comment()
    {
        AuthorId = user.Identity.Name,
        Content = req.Content,
        CreatedAt = DateTime.UtcNow,
    };
    
    var result = await commentService.WriteComment(req.PostId, comment);
    
    return result ? Results.Ok() : Results.BadRequest("Comment Failed");
}).RequireAuthorization().RequireRateLimiting("per-ip");

app.MapGet("/getComments/{postid}/chunk/{chunkid}", async (IPostsService posts, string postid, string chunkid) =>
{
    if(!int.TryParse(chunkid, out int parsedChunkId)) return Results.BadRequest("Chunk ID is invalid");
    var result = await posts.GetPostCommentsByChunk(postid, parsedChunkId);
    return result.result ? Results.Ok(result.comments) : Results.BadRequest("Post or chunk not found");
}).RequireAuthorization().RequireRateLimiting("per-ip");

app.Run();