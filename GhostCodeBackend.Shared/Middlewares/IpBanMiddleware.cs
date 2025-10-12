using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

public sealed class IpBanMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, DateTime> _bans = new();

    public IpBanMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_bans.TryGetValue(ip, out var bannedUntil))
        {
            if (DateTime.UtcNow < bannedUntil)
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                await ctx.Response.WriteAsync("Banned");
                return;
            }
    
            _bans.TryRemove(ip, out _);
        }

        await _next(ctx);
    }
    
    public static void Ban(string ip, TimeSpan duration) =>
        _bans.AddOrUpdate(ip,
            _ => DateTime.UtcNow.Add(duration),
            (_, _) => DateTime.UtcNow.Add(duration));
}