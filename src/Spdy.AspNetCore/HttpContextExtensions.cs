using Microsoft.AspNetCore.Http;

namespace Spdy.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static ISpdyManager Spdy(
            this HttpContext httpContext)
            => new DefaultSpdyManager(httpContext.Features);
    }
}