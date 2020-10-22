using Microsoft.AspNetCore.Builder;
using Port.Server.Spdy.AspNet;

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