using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace GhostCodeBackend.Shared.Middlewares;

public sealed class IpBanMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private const string BanKeyPrefix = "ipban:";

    public IpBanMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var banKey = $"{BanKeyPrefix}{ip}";

        var banValue = await _cache.GetStringAsync(banKey);
        
        if (banValue != null)
        {
            // Проверяем, не истек ли бан
            if (long.TryParse(banValue, out var banUntilTicks))
            {
                var banUntil = new DateTime(banUntilTicks, DateTimeKind.Utc);
                
                if (DateTime.UtcNow < banUntil)
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await ctx.Response.WriteAsync("Banned");
                    return;
                }
                
                // Бан истек, удаляем
                await _cache.RemoveAsync(banKey);
            }
        }

        await _next(ctx);
    }
    
    public static async Task BanAsync(IDistributedCache cache, string? ip, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(ip) || ip == "unknown")
            return;

        var banKey = $"{BanKeyPrefix}{ip}";
        var banUntil = DateTime.UtcNow.Add(duration);
        var banValue = banUntil.Ticks.ToString();
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        };
        
        await cache.SetStringAsync(banKey, banValue, options);
    }
}