
using GhostCodeBackend.AccountsManagementService.Repositories;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.RPC.Tracker;
using GhostCodeBakend.AccountsManagementService.Rpc;
using GhostCodeBakend.AccountsManagementService.Services;
using GhostCodeBakend.AccountsManagementService.Utils;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation())
    .UseOtlpExporter();

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
        config.PermitLimit   = 5;          // сколько
        config.Window        = TimeSpan.FromMinutes(1);
        config.QueueLimit    = 0;           // без очереди – сразу 429
        config.AutoReplenishment = true;
    });
    
    opt.OnRejected = (ctx, ct) =>
    {
        var ip = ctx.HttpContext.Connection.RemoteIpAddress?.ToString();
        IpBanMiddleware.Ban(ip, TimeSpan.FromMinutes(10));
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };
});

builder.AddRabbitMQClient("rabbitmq");

// Utils
builder.Services.AddSingleton<IHasher, Hasher>();
builder.Services.AddSingleton<IRandomWordGenerator, RandomWordGenerator>();

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

app.UseMiddleware<IpBanMiddleware>();



app.MapDefaultEndpoints();
app.UseRateLimiter();
app.UseCors("AllowFrontend");

await app.Services.GetRequiredService<IRabbitMQService>().InitializeAsync();
await app.Services.GetRequiredService<IRpcConsumer>().InitConsume();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/register", async (RegisterRequestDTO req, IAccountsService accounts) =>
{
    var result = await accounts.RegisterAsync(req);
    
    if(!result.result) return Results.BadRequest("Account registration failed");
    
    return Results.Ok(new
    {
        recoveryCode = result.recoveryCode,
        refreshToken = result.newRefresh
    });
}).RequireRateLimiting("per-ip");

app.MapPost("/login", async (LoginRequestDTO req, IAccountsService accounts) =>
{
    var results = await accounts.LoginAsync(req);
    
    return results.result ? Results.Ok(new
    {
        data = results.userData,
        refreshToken = results.newRefresh
    }
    ) :  Results.BadRequest("Login failed");
}).RequireRateLimiting("per-ip");

app.MapPost("/recovery", async (AccountRecoveryRequestDTO req, IAccountsService accounts) =>
{
    var results = await accounts.PasswordReset(req.Login, req.RecoveryCode, req.NewPassword);
    
    return results.result ? Results.Ok(new
    {
        newRecovery = results.newRecoveryCode,
    }) : Results.BadRequest("Password reset failed");
}).RequireRateLimiting("per-ip");




app.Run();
