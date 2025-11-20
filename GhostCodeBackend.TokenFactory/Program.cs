using System.Security.Claims;
using System.Text;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shared.CfgObjects;
using TokenFactory.Repositories;
using TokenFactory.Rpc;
using TokenFactory.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration cfg = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRabbitMQClient("rabbitmq");
builder.AddRedisDistributedCache("redis");

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRpcResponser, RpcResponser>();
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(cfg.GetConnectionString("mongodb")));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("main");
});

JwtOptions jwtOpt = 
    new JwtOptions()
    {
        Audience = cfg["JWTAudience"],
        Issuer = cfg["JWTIssuer"],
        Key = cfg["JWTKey"],
        ExpireMinutes = int.Parse(cfg["JWTExpireMinutes"])
    };

builder.Services.AddSingleton<JwtOptions>(_ =>
{
    JwtOptions opt = new JwtOptions()
    {
        Audience = cfg["JWTAudience"],
        Issuer = cfg["JWTIssuer"],
        Key = cfg["JWTKey"],
        ExpireMinutes = int.Parse(cfg["JWTExpireMinutes"])
    };
    return opt;
});

builder.Services.AddSingleton<IRefreshTokensRepository, RefreshTokensRepository>();
builder.Services.AddScoped<IRefreshTokensService, RefreshTokensService>(
    _ => new RefreshTokensService(_.GetRequiredService<IRefreshTokensRepository>(), 
    int.Parse(cfg["RefreshExpireDays"])));
builder.Services.AddScoped<IJwtService, JwtService>();


builder.AddDefaultAuthorization(builder.Configuration);
builder.AddDefaultRateLimits(5, 60*3);
builder.AddDefaultCors();

builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.AddServiceDefaults();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultRateLimits();
app.UseDefaultCors();
app.UseDefaultAuth();
app.MapDefaultEndpoints();

await app.Services.GetRequiredService<IRabbitMQService>().InitializeAsync();
await app.Services.GetRequiredService<IRpcResponser>().InitResponses();

app.MapGet("/checkToken", () =>
{
    return Results.Ok("valid");
}).RequireAuthorization().RequireRateLimiting("per-ip");

app.MapPost("/refresh", async (RefreshRequestDTO req, IJwtService jwtService) =>
{
    var result = await jwtService.CreateToken(req.Token);

    return result.IsSuccess
        ? Results.Ok(new
        {
            newRefresh = result.Value.RefreshToken,
            newJwt = result.Value.Token
        }) : Results.BadRequest("invalid refresh token");
}).RequireRateLimiting("per-ip");


app.Run();
