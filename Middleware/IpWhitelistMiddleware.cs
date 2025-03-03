using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _allowedIps;

    public IpWhitelistMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _allowedIps = configuration.GetSection("AllowedIPs").Get<List<string>>() ?? new List<string>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;

        if (remoteIp != null && remoteIp.Equals(IPAddress.IPv6Loopback))
        {
            remoteIp = IPAddress.Parse("127.0.0.1");
        }

        if (!_allowedIps.Any(ip => IPAddress.Parse(ip).Equals(remoteIp)))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"message\": \"Access denied. Your IP is not allowed." + remoteIp.ToString() + "\"}");
            return;
        }

        await _next(context);
    }
}
