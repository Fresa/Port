using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;

namespace Kubernetes.Test.API.Server
{
    internal sealed class UpgradeMessageHandler : HttpMessageHandler
    {
        private readonly TestServer _testServer;

        public UpgradeMessageHandler(
            TestServer testServer) => _testServer = testServer;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestContent =
                request.Content ?? new StreamContent(Stream.Null);

            var httpContext = await _testServer.SendAsync(
                                                   ConfigureContext,
                                                   cancellationToken)
                                               .ConfigureAwait(false);

            var response = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)httpContext.Response.StatusCode,
                ReasonPhrase = httpContext.Features.Get<IHttpResponseFeature>()
                                          .ReasonPhrase,
                RequestMessage = request
            };

            var duplexStreamFeature =
                httpContext.Features.Get<IHttpDuplexStreamFeature>();
            if (duplexStreamFeature != null &&
                duplexStreamFeature.Body != Stream.Null)
            {
                response.Content =
                    new DuplexStreamContent(duplexStreamFeature.Body);
                // This avoids anyone setting a buffer for the content which will override the stream
                await response.Content.ReadAsStreamAsync(cancellationToken)
                              .ConfigureAwait(false);
            }
            else
            {
                response.Content = new StreamContent(httpContext.Response.Body);
            }

            foreach (var (key, value) in httpContext.Response.Headers)
            {
                if (response.Headers.TryAddWithoutValidation(
                    key, (IEnumerable<string>)value))
                {
                    continue;
                }

                var success = response.Content.Headers.TryAddWithoutValidation(
                    key, (IEnumerable<string>)value);
                Contract.Assert(success, "Bad header");
            }

            return response;

            void ConfigureContext(
                HttpContext context)
            {
                var req = context.Request;

                if (request.Version == HttpVersion.Version20)
                {
                    // https://tools.ietf.org/html/rfc7540
                    req.Protocol = "HTTP/2";
                }
                else
                {
                    req.Protocol = "HTTP/" + request.Version.ToString(2);
                }

                req.Method = request.Method.ToString();

                req.Scheme = request.RequestUri.Scheme;

                foreach (var (key, value) in request.Headers)
                {
                    req.Headers.Append(key, value.ToArray());
                }

                if (!req.Host.HasValue)
                {
                    // If Host wasn't explicitly set as a header, let's infer it from the Uri
                    req.Host = HostString.FromUriComponent(request.RequestUri);
                    if (request.RequestUri.IsDefaultPort)
                    {
                        req.Host = new HostString(req.Host.Host);
                    }
                }

                req.Path = PathString.FromUriComponent(request.RequestUri);
                req.PathBase = PathString.Empty;
                var pathBase = _testServer.BaseAddress == null
                    ? PathString.Empty
                    : PathString.FromUriComponent(_testServer.BaseAddress);
                if (req.Path.StartsWithSegments(pathBase, out var remainder))
                {
                    req.Path = remainder;
                    req.PathBase = pathBase;
                }

                req.QueryString =
                    QueryString.FromUriComponent(request.RequestUri);

                foreach (var (key, value) in requestContent.Headers)
                {
                    req.Headers.Append(key, value.ToArray());
                }
            }
        }
    }
}