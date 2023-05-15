using MiroslavGPT.Admin.API.Settings;

namespace MiroslavGPT.Admin.API.Middlewares;

public class SingleKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuthSettings _authSettings;

    public SingleKeyAuthenticationMiddleware(RequestDelegate next, IAuthSettings authSettings)
    {
        _next = next;
        _authSettings = authSettings;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var key = authHeader.ToString().Split(" ").Last();
            if (key == _authSettings.Secret)
            {
                await _next(context);
                return;
            }
        }

        context.Response.StatusCode = 401;
    }
}

public static class SingleKeyAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseSingleKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SingleKeyAuthenticationMiddleware>();
    }
}