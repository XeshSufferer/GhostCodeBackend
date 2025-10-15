using System.Text;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Shared.CfgObjects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

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

builder.AddServiceDefaults();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole(Role.Admin.ToString()));
    
    options.AddPolicy("OnlyInDevGroup", policy =>
        policy.RequireRole(Role.Admin.ToString(), Role.Developer.ToString()));
    
    options.AddPolicy("Testing", policy =>
        policy.RequireRole(Role.Admin.ToString(), Role.Developer.ToString(), Role.Tester.ToString()));
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

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<IpBanMiddleware>();
app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.UseRateLimiter();



app.MapPost("/CreatePromocode", (PromocodeCreationRequestDTO req) =>
{
    
}).RequireCors("AllowFrontend").RequireAuthorization("AdminOnly").RequireRateLimiting("per-ip");



app.Run();
