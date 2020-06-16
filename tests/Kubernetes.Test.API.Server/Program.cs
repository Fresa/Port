using System.Runtime.CompilerServices;
using System.Threading;
using Log.It;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using ILogger = Log.It.ILogger;

namespace Kubernetes.Test.API.Server
{
    public class Program
    {
        public static void Main(
            string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(
            string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }

    internal sealed class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ILogger _logger =
            LogFactory.Create<HttpResponseExceptionFilter>();

        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            if (context.Exception != null)
            {
                _logger.Error(context.Exception, "Unhandled exception");
            }
        }
    }
}