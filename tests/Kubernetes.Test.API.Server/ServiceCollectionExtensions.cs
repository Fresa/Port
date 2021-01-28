using Microsoft.Extensions.DependencyInjection;
using Spdy.AspNetCore;

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