using Microsoft.AspNetCore.Http;

namespace Port.Server.Spdy.AspNet
{
    public static class HttpContextExtensions
    {
        public static ISpdyManager Spdy(
            this HttpContext httpContext)
            => new DefaultSpdyManager(httpContext.Features);
    }
}