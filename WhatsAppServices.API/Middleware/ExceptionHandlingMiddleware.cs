using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.Json;

namespace WhatsAppServices.API.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _host;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _rateLimiter = TimeSpan.FromSeconds(30);

        public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment host, IMemoryCache cache)
        {
            _next = next;
            _host = host;
            _cache = cache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                ApplySecurity(httpContext);
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";

                if (_host.IsDevelopment())
                    await httpContext.Response.WriteAsync(JsonSerializer.Serialize( new { message = ex.Message, details = ex.StackTrace }));
                else
                    await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Something wrong try again later" })); 
            }
        }

        // Legacy rate limiting logic (based on IMemoryCache and IP)
        // Replaced by built-in ASP.NET Core Rate Limiter using AddRateLimiter in Program.cs
        // Use app.UseRateLimiter() in the pipeline and configure it via services.AddRateLimiter()
        // NOTE: This method is no longer used and can be safely removed after verifying the new setup.
        private bool IsRequestAllowed(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var cacheKey = $"Rate:{ip}";
            var date = DateTime.Now;
            var (timeStamp, count) = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _rateLimiter;
                return (timeStamp: date, count: 0);
            });

            if (date - timeStamp < _rateLimiter)
            {
                if (count >= 8)
                {
                    return false;
                }
                _cache.Set(cacheKey, (timeStamp, count += 1), _rateLimiter);
            }
            else
            {
                _cache.Set(cacheKey, (timeStamp, count), _rateLimiter);
            }

            return true;

        }

        private void ApplySecurity(HttpContext context)
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-XSS-Protection"] = "1;mode=block";
            context.Response.Headers["X-Frame-Options"] = "DENY";
        }

    }
    /// <summary>
    /// Exception Handling Middleware Extensions
    /// </summary>
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
