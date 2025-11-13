using System.Text;
using GhostCodeBackend.NotificationService.Hubs;
using GhostCodeBackend.NotificationService.RPC;
using GhostCodeBackend.NotificationService.Services;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.RPC.Tracker;
using GhostCodeBackend.Shared.Сache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Shared.CfgObjects;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.AddDefaultCors();
builder.AddDefaultRateLimits(5, 10);
builder.AddServiceDefaults();
builder.AddDefaultAuthorization(builder.Configuration);

builder.Services.AddSingleton<IRpcConsumer, RpcConsumer>();
builder.Services.AddScoped<IUniversalRequestTracker, UniversalRequestTracker>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<ICacheService, RedisService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


builder.AddRabbitMQClient("rabbitmq");
builder.AddRedisDistributedCache("redis");

var app = builder.Build();


app.MapDefaultEndpoints();
app.UseDefaultRateLimits();
app.UseDefaultCors();

await app.Services.GetRequiredService<IRabbitMQService>().InitializeAsync();
await app.Services.GetRequiredService<IRpcConsumer>().InitConsume();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.MapHub<NotificationHub>("/notifications");

app.Run();
