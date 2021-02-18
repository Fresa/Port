using Microsoft.AspNetCore.Builder;
using Spdy.AspNetCore;

namespace Kubernetes.Test.API.Server
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSpdy(
            this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<SpdyMiddleware>();
        }
    }
}