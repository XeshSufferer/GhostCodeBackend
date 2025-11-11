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


var cfg = builder.Configuration;

JwtOptions jwtOpt = 
    new JwtOptions()
    {
        Audience = cfg["JWTAudience"],
        Issuer = cfg["JWTIssuer"],
        Key = cfg["JWTKey"],
        ExpireMinutes = int.Parse(cfg["JWTExpireMinutes"])
    };

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

builder.Services.AddSingleton<IRpcConsumer, RpcConsumer>();
builder.Services.AddScoped<IUniversalRequestTracker, UniversalRequestTracker>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<ICacheService, RedisService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


builder.AddRabbitMQClient("rabbitmq");
builder.AddRedisDistributedCache("redis");

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


app.UseHttpsRedirection();

app.MapHub<NotificationHub>("/notifications");

app.Run();
