using System.Threading.Tasks;
using k8s.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Port.Server.Hosting
{
    internal static class HostEnvironmentExtensions
    {
        internal static Task HandleExceptions(
            this IHostEnvironment env,
            HttpContext context)
        {
            var exceptionObject =
                context.Features.Get<IExceptionHandlerFeature>();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse Can be null
            if (exceptionObject == null)
            {
                return Task.CompletedTask;
            }

            var message = exceptionObject.Error switch
            {
                KubeConfigException ex when
                    ex.Message.StartsWith("Refresh not supported") =>
                    "Refresh tokens are not supported. Please refresh the Kubernetes access token and try again",
                KubeConfigException ex =>
                    $"Kubernetes config error: {ex.Message}",
                _ => env.IsDevelopment()
                    ? exceptionObject.Error.ToString()
                    : "Unknown error occurred"
            };

            return context.Response.WriteAsync(message);
        }
    }
}