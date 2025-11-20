
using GhostCodeBackend.AccountsManagementService.Repositories;
using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.RPC.Tracker;
using GhostCodeBackend.Shared.Сache;
using GhostCodeBackend.AccountsManagementService.Rpc;
using GhostCodeBackend.AccountsManagementService.Services;
using GhostCodeBackend.AccountsManagementService.Utils;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDefaultCors();

builder.AddDefaultRateLimits(5, 10);

builder.AddRabbitMQClient("rabbitmq");
builder.AddRedisDistributedCache("redis");

// Utils
builder.Services.AddSingleton<IHasher, Hasher>();
builder.Services.AddSingleton<IRandomWordGenerator, RandomWordGenerator>();

// Cache
builder.Services.AddSingleton<ICacheService, RedisService>();

// Repo&Services
builder.Services.AddSingleton<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<IAccountsService,  AccountsService>();

// RPC
builder.Services.AddSingleton<IUniversalRequestTracker, UniversalRequestTracker>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRpcConsumer, RpcConsumer>();

// Mongo
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetConnectionString("mongodb")));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("main");
});




builder.AddServiceDefaults();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultRateLimits();
app.MapDefaultEndpoints();
app.UseDefaultCors();

await app.Services.GetRequiredService<IRabbitMQService>().InitializeAsync();
await app.Services.GetRequiredService<IRpcConsumer>().InitConsume();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/register", async (RegisterRequestDTO req, IAccountsService accounts, IRabbitMQService rabbit) =>
{
    var result = await accounts.RegisterAsync(req);
    
    if(!result.result) return Results.BadRequest("Account registration failed");
    
    var msg = new Message<RegisterRequestDTO>()
    {
        Data = req
    };
    
    await rabbit.SendMessageAsync(msg, "GitUsage.CreateAccount");
    
    
    return Results.Ok(new
    {
        recoveryCode = result.recoveryCode,
        refreshToken = result.newRefresh,
        data = new UserData().MapFromDomainUser(result.userObj)
    });
}).RequireRateLimiting("per-ip");

app.MapPost("/login", async (LoginRequestDTO req, IAccountsService accounts) =>
{
    var results = await accounts.LoginAsync(req);
    
    return results.IsSuccess ? Results.Ok(new
    {
        data = results.Value.userData,
        refreshToken = results.Value.newRefresh
    }
    ) :  Results.BadRequest("Login failed");
}).RequireRateLimiting("per-ip");

app.MapPost("/recovery", async (AccountRecoveryRequestDTO req, IAccountsService accounts) =>
{
    var results = await accounts.PasswordReset(req.Login, req.RecoveryCode, req.NewPassword);
    
    return results.IsSuccess ? Results.Ok(new
    {
        newRecovery = results.Value,
    }) : Results.BadRequest("Password reset failed");
}).RequireRateLimiting("per-ip");

app.MapGet("/getData/{id}", async (string id, IAccountsService accounts) =>
{
    var data = await accounts.GetUserdata(id);
    
    return data.IsSuccess ? Results.Ok(new { data = data.Value }) : Results.BadRequest("User not found");
});

app.Run();
