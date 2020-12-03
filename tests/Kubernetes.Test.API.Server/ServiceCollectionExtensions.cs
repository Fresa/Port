using Microsoft.Extensions.DependencyInjection;
using Port.Server.Spdy.AspNet;

namespace Kubernetes.Test.API.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpdy(
            this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTransient<SpdyMiddleware>();
        }
    }
}