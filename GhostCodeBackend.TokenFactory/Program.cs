using System.Security.Claims;
using System.Text;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TokenFactory.CfgObjects;
using TokenFactory.Repositories;
using TokenFactory.Rpc;
using TokenFactory.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration cfg = builder.Configuration;


builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation())
    .UseOtlpExporter();


builder.AddRabbitMQClient("rabbitmq");

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


builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.AddServiceDefaults();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

await app.Services.GetRequiredService<IRabbitMQService>().InitializeAsync();
await app.Services.GetRequiredService<IRpcResponser>().InitResponses();

app.MapGet("/checkToken", () =>
{
    return Results.Ok("valid");
}).RequireAuthorization();

app.MapPost("/refresh", async (RefreshRequestDTO req, IJwtService jwtService) =>
{
    var result = await jwtService.CreateToken(req.Token);

    return result.result
        ? Results.Ok(new
        {
            newRefresh = result.newRefresh,
            newJwt = result.newJwt
        }) : Results.BadRequest("invalid refresh token");
});


app.Run();
