using Microsoft.AspNetCore.Mvc.Filters;
using Spdy.Logging;

namespace Spdy.IntegrationTests.TestFramework
{
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