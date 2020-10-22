using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Kubernetes.Test.API.Server
{
    /// <summary>
    /// Used to simulate upgrades during testing through the TestServer
    /// </summary>
    internal sealed class UpgradeTestMiddleware
    {
        private readonly RequestDelegate _next;

        public UpgradeTestMiddleware(
            RequestDelegate next) => _next = next;

        // ReSharper disable once UnusedMember.Global
        // Implicit middleware
        public Task InvokeAsync(
            HttpContext context)
        {
            var middlewareCompletion = new TaskCompletionSource<bool>();

            var upgradeHandshake = new UpgradeHandshake(context);
            upgradeHandshake.OnUpgraded +=
                () => middlewareCompletion.TrySetResult(true);
            context.Features.Set<IHttpUpgradeFeature>(upgradeHandshake);
            context.Features.Set<IHttpDuplexStreamFeature>(upgradeHandshake);

            try
            {
                _next.Invoke(context)
                     .ContinueWith(
                         task =>
                         {
                             if (task.IsCanceled)
                             {
                                 return middlewareCompletion.TrySetCanceled();
                             }

                             if (task.IsFaulted)
                             {
                                 return middlewareCompletion.TrySetException(
                                     task.Exception ?? new Exception(
                                         "Unknown failure from middleware"));
                             }

                             return middlewareCompletion.TrySetResult(true);
                         });
            }
            catch (Exception ex)
            {
                middlewareCompletion.TrySetException(ex);
            }

            return middlewareCompletion.Task;
        }
    }
}