using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Log.It;

namespace Port.Server
{
    internal sealed class LogItHttpMessageHandlerDecorator : DelegatingHandler
    {
        private readonly ILogger _logger = LogFactory.Create<LogItHttpMessageHandlerDecorator>();

        internal LogItHttpMessageHandlerDecorator(HttpMessageHandler httpMessageHandler) :
            base(httpMessageHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Content != null)
            {
                var requestContent = await request.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                _logger.Trace($"Request: {requestContent}");
            }

            var response = await base
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            var responseContent = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            _logger.Trace($"Response: {responseContent}");

            return response;
        }
    }
}