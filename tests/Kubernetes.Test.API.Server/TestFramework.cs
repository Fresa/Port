using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Kubernetes.Test.API.Server.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NLog.Extensions.Logging;
using NLog.Web;
using Port.Server.Spdy.AspNet;

namespace Kubernetes.Test.API.Server
{
    public sealed class TestFramework : IAsyncDisposable
    {
        private readonly IHostBuilder _hostBuilder;
        private IHost? _host;

        private TestServer GetTestServer()
        {
            var testServer = _host.GetTestServer();
            testServer.PreserveExecutionContext = true;
            return testServer;
        }

        private TestFramework(
            IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public Uri BaseAddress => GetTestServer()
            .BaseAddress;

        internal void Start()
        {
            _host = _hostBuilder.ConfigureServices(
                    collection => collection.AddSingleton(this))
                .Build();
            _host.StartAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public static TestFramework Start(
            params string[] args)
        {
            IConfiguration configuration = default!;
            var hostBuilder = Program.CreateHostBuilder(args)
                .ConfigureAppConfiguration(
                    (
                        context,
                        configurationBuilder) =>
                    {
                        configuration = context.Configuration;
                    })
                .ConfigureServices(
                    collection =>
                    {
                        collection.AddLogging(
                            builder =>
                            {
                                builder.ClearProviders();
                                builder.AddNLog(configuration);
                            });
                        collection.AddControllers(
                            options =>
                                options.Filters.Add(
                                    new HttpResponseExceptionFilter()));
                    })
                .ConfigureWebHost(builder => builder.UseTestServer())
                .UseNLog();

            var testFramework = new TestFramework(hostBuilder);
            testFramework.Start();
            return testFramework;
        }

        public HttpMessageHandler CreateHttpMessageHandler()
        {
            return new UpgradeMessageHandler(GetTestServer());
        }

        public WebSocketClient CreateWebSocketClient()
        {
            return GetTestServer()
                .CreateWebSocketClient();
        }

        public PodSubscriptions Pod { get; } =
            new PodSubscriptions();

        public async ValueTask DisposeAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync()
                    .ConfigureAwait(false);
            }
            _host?.Dispose();
        }
    }

    internal sealed class UpgradeMessageHandler : HttpMessageHandler
    {
        private readonly TestServer _testServer;

        public UpgradeMessageHandler(TestServer testServer)
        {
            _testServer = testServer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestContent = request.Content ?? new StreamContent(Stream.Null);

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
                    req.Protocol = "HTTP/" + request.Version.ToString(fieldCount: 2);
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

                req.QueryString = QueryString.FromUriComponent(request.RequestUri);

                foreach (var (key, value) in requestContent.Headers)
                {
                    req.Headers.Append(key, value.ToArray());
                }
            }

            var httpContext = await _testServer.SendAsync(
                                                   ConfigureContext, cancellationToken)
                                               .ConfigureAwait(false);

            var response = new HttpResponseMessage();
            response.StatusCode = (HttpStatusCode)httpContext.Response.StatusCode;
            response.ReasonPhrase = httpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase;
            response.RequestMessage = request;

            var duplexStreamFeature = httpContext.Features.Get<IHttpDuplexStreamFeature>();
            if (duplexStreamFeature != null && duplexStreamFeature.Body != Stream.Null)
            {
                response.Content = new DuplexStreamContent(duplexStreamFeature.Body);
                await response.Content.ReadAsStreamAsync()
                              .ConfigureAwait(false);
            }
            else
            {
                response.Content = new StreamContent(httpContext.Response.Body);
            }

            foreach (var header in httpContext.Response.Headers)
            {
                if (!response.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string>)header.Value))
                {
                    bool success = response.Content.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string>)header.Value);
                    Contract.Assert(success, "Bad header");
                }
            }
            return response;
        }
    }

    internal sealed class DuplexStreamContent : HttpContent
    {
        private readonly Stream _stream;

        public DuplexStreamContent(Stream stream)
        {
            _stream = stream;
        }
        
        protected override Task<Stream> CreateContentReadStreamAsync() => Task.FromResult(_stream);

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext context)
            => Task.CompletedTask;

        protected override bool TryComputeLength(
            out long length)
        {
            length = 0;
            return false;
        }
    }

    /// <summary>
    /// Used to simulate upgrades during testing through the TestServer
    /// </summary>
    internal sealed class UpgradeTestMiddleware
    {
        private readonly RequestDelegate _next;

        public UpgradeTestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        // Implicit middleware
        public Task InvokeAsync(
            HttpContext context)
        {
            var middlewareCompletion = new TaskCompletionSource<bool>();

            var upgradeHandshake = new UpgradeHandshake(context);
            upgradeHandshake.OnUpgraded += () => middlewareCompletion.TrySetResult(true);
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
                                     task.Exception ?? new Exception("Unknown failure from middleware"));
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


    internal sealed class UpgradeHandshake : IHttpUpgradeFeature, IHttpDuplexStreamFeature
    {
        private readonly HttpContext _context;
        private const string ConnectionUpgrade = "Upgrade";
        private bool? _isSpdyRequest;

        private static readonly string[] NeededHeaders =
        {
            HeaderNames.Connection,
            HeaderNames.Upgrade
        };

        public UpgradeHandshake(HttpContext context)
        {
            _context = context;
        }

        public Task<Stream> UpgradeAsync() 
        {
            if (!IsUpgradableRequest)
            {
                throw new InvalidOperationException("Not an upgrade request.");
            }

            if (_context.Response.HasStarted)
            {
                throw new InvalidOperationException("The response has already started");
            }

            var stream1 = new MemoryStream();
            var stream2 = new MemoryStream();
            var client = new CrossWiredStream(stream1, stream2);
            var server = new CrossWiredStream(stream2, stream1);

            _context.Response.StatusCode =
                StatusCodes.Status101SwitchingProtocols;
            Body = client;
            OnUpgraded.Invoke();
            return Task.FromResult<Stream>(server);
        }

        internal event Action OnUpgraded = () => {};

        public Stream Body { get; private set; } = Stream.Null;

        public bool IsUpgradableRequest
        {
            get
            {
                if (_isSpdyRequest == null)
                {
                    var headers =
                        new List<KeyValuePair<string, string>>();
                    foreach (var headerName in NeededHeaders)
                    {
                        headers.AddRange(
                            _context
                                .Request.Headers
                                .GetCommaSeparatedValues(headerName)
                                .Select(
                                    value
                                        => new KeyValuePair<string,
                                            string>(
                                            headerName, value)));
                    }

                    _isSpdyRequest = CheckSupportedUpgradeRequest(headers);
                }

                return _isSpdyRequest.Value;
            }
        }

        private static bool CheckSupportedUpgradeRequest(
            IEnumerable<KeyValuePair<string, string>> headers)
        {
            bool validUpgrade = false, validConnection = false;

            foreach (var (key, value) in headers)
            {
                if (string.Equals(
                    HeaderNames.Connection, key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(
                        ConnectionUpgrade, value,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        validConnection = true;
                    }
                }
                else if (string.Equals(
                    HeaderNames.Upgrade, key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    validUpgrade = true;
                }
            }

            return validConnection && validUpgrade;
        }
    }

    internal interface IHttpDuplexStreamFeature
    {
        public Stream Body { get; }
    }

    internal sealed class CrossWiredStream : Stream
    {
        private readonly Stream _read;
        private readonly Stream _write;

        public CrossWiredStream(Stream read, Stream write)
        {
            _read = read;
            _write = write;
        }

        public override void Flush()
        {
            _write.Flush();
        }

        public override int Read(
            byte[] buffer,
            int offset,
            int count)
            => _read.Read(buffer, offset, count);

        public override long Seek(
            long offset,
            SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(
            long value)
        => throw new NotSupportedException();

        public override void Write(
            byte[] buffer,
            int offset,
            int count)
        {
            _write.Write(buffer, offset, count);
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _write.WriteAsync(buffer, offset, count, cancellationToken);
        }
        public override void WriteByte(byte value)
        {
            _write.WriteByte(value);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _write.BeginWrite(buffer, offset, count, callback, state);
        }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}