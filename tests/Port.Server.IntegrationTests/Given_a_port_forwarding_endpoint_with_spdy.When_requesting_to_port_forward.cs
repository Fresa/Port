using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kubernetes.Test.API.Server.Subscriptions.Models;
using Microsoft.FeatureManagement;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.TestFramework;
using Spdy;
using Test.It;
using Xunit;
using Xunit.Abstractions;
using ReadResult = System.IO.Pipelines.ReadResult;

namespace Port.Server.IntegrationTests
{
    public partial class Given_a_port_forwarding_endpoint_with_spdy
    {
        public partial class
            When_requesting_to_port_forward : XUnit2ServiceSpecificationAsync<
                PortServerHost>
        {
            private HttpResponseMessage _response = default!;
            private Fixture _fixture = default!;

            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

            protected override void Given(
                IServiceContainer configurer)
            {
                configurer.RegisterSingleton<IFeatureManager>(() => new TestFeatureManager((nameof(Features.PortForwardingWithSpdy), true)));
                _fixture = DisposeAsyncOnTearDown(new Fixture(configurer));
                _fixture.PortForwardingSocket.On<byte[]>(
                    bytes => { _fixture.PortForwardResponseReceived(bytes); });

                _fixture.KubernetesApiServer.Pod.PortForward.OnConnected(
                    new PortForward("test", "pod1", 2001), async (
                        session,
                        cancellationToken) =>
                    {
                        using var stream = await session
                                           .ReceiveAsync(cancellationToken)
                                           .ConfigureAwait(false);

                        ReadResult result;
                        do
                        {
                            result = await _fixture.ReceiveAsync(
                                                       stream, cancellationToken)
                                                   .ConfigureAwait(false);
                            if (await _fixture
                                      .TrySendResponseAsync(
                                          stream, cancellationToken)
                                      .ConfigureAwait(false))
                            {
                                await stream.SendLastAsync(
                                                ReadOnlyMemory<byte>.Empty,
                                                cancellationToken: cancellationToken)
                                            .ConfigureAwait(false);
                                return;
                            }
                        } while (cancellationToken
                                     .IsCancellationRequested ==
                                 false &&
                                 result.HasMoreData());
                    });
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _response = await Server.CreateHttpClient()
                    .PostAsJsonAsync(
                        "service/test/portforward",
                        new Shared.PortForward(
                                "test",
                                pod: "pod1",
                                service: "service1",
                                ProtocolType.Tcp,
                                2001)
                        { LocalPort = 1000 }, cancellationToken)
                    .ConfigureAwait(false);

                var client =
                    await _fixture.PortForwardingSocket
                        .ConnectAsync(
                            new ByteArrayMessageClientFactory(), IPAddress.IPv6Any,
                            1000, ProtocolType.Tcp, cancellationToken)
                        .ConfigureAwait(false);
                await using (client.ConfigureAwait(false))
                {
                    await client.SendAsync(
                                    Encoding.ASCII.GetBytes(_fixture.Request),
                                    cancellationToken)
                                .ConfigureAwait(false);

                    await _fixture.WaitForResponseAsync(cancellationToken)
                                  .ConfigureAwait(false);
                }
            }

            [Fact(
                DisplayName =
                    "k8s api server should receive the request message sent")]
            public void TestReceiveRequestMessage()
            {
                _fixture.MessageReceived.Should()
                    .HaveCount(_fixture.Request.Length);
                Encoding.ASCII.GetString(
                        _fixture.MessageReceived.GetRange(
                                0, _fixture.Request.Length)
                            .ToArray())
                    .Should()
                    .Be(_fixture.Request);
            }

            [Fact(DisplayName = "It should start port forwarding")]
            public void
                TestStartPortForwarding()
            {
                _response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
            }

            [Fact(DisplayName = "It should receive a http response")]
            public void TestReceiveResponse()
            {
                _fixture.PortForwardResponse.Should()
                    .Be(
                        _fixture.FragmentedResponses.Aggregate(
                            "", (
                                total,
                                response) => total + response));
            }

            internal sealed class Fixture : IAsyncDisposable
            {
                public Fixture(
                    IServiceContainer container)
                {
                    PortForwardingSocket =
                        SocketTestFramework.SocketTestFramework.InMemory();
                    container.RegisterSingleton(
                        () => PortForwardingSocket
                            .NetworkServerFactory);

                    KubernetesApiServer =
                        global::Kubernetes.Test.API.Server.TestFramework.Start();
                    container.RegisterSingleton(
                        () => KubernetesApiServer
                            .CreateKubernetesConfiguration());
                }

                internal InMemorySocketTestFramework PortForwardingSocket
                {
                    get;
                }

                internal global::Kubernetes.Test.API.Server.TestFramework
                    KubernetesApiServer
                { get; }

                internal string Request { get; } =
"POST /cgi-bin/process.cgi HTTP/1.1\r\n" +
"User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)\r\n" +
"Host: www.tutorialspoint.com\r\n" +
"Content-Type: text/xml; charset=utf-8\r\n" +
"Content-Length: length\r\n" +
"Accept-Language: en-us\r\n" +
"Accept-Encoding: gzip, deflate\r\n" +
"Connection: Keep-Alive\r\n" +
"\r\n" +
"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
"<string xmlns = \"http://clearforest.com/\">string</string>";

                internal string[] FragmentedResponses { get; } =
                {
"HTTP/1.1 400 Bad Request\r\n" +
"Date: Sun, 18 Oct 2012 10:36:20 GMT\r\n" +
"Server: Apache/2.2.14 (Win32)\r\n" +
"Content-Length: 327\r\n" +
"Content-Type: text/html; charset=iso-8859-1\r\n" +
"Connection: Closed\r\n" +
"\r\n" +
"<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">",

"<html>\r\n" +
"<head>\r\n" +
"   <title>400 Bad Request</title>\r\n" +
"</head>\r\n" +
"<body>\r\n" +
"   <h1>Bad Request</h1>\r\n" +
"   <p>Your browser sent a request that this server could not understand.</p>\r\n" +
"   <p>The request line contained invalid characters following the protocol string.</p>\r\n" +
"</body>\r\n" +
"</html>"
                };

                private readonly IMemoryOwner<byte> _memoryOwner =
                    MemoryPool<byte>.Shared.Rent(65536);

                private Memory<byte> Memory => _memoryOwner.Memory;

                internal readonly List<byte> MessageReceived =
                    new List<byte>();

                internal string PortForwardResponse { get; private set; } = "";

                internal void PortForwardResponseReceived(
                    byte[] buffer)
                {
                    PortForwardResponse +=
                        Encoding.ASCII.GetString(buffer);
                    _responseReceived.Release();
                }

                private readonly SemaphoreSlim _responseReceived =
                    new SemaphoreSlim(0);


                internal async Task WaitForResponseAsync(
                    CancellationToken cancellationToken)
                {
                    var timeout = TimeSpan.FromSeconds(25);
                    while (PortForwardResponse.Length < FragmentedResponses.Sum(response => response.Length))
                    {
                        if (await _responseReceived.WaitAsync(
                                timeout, cancellationToken)
                            .ConfigureAwait(false))
                        {
                            continue;
                        }

                        throw new TimeoutException($"Waited {timeout:ss}s");
                    }
                }

                internal async ValueTask<bool> TrySendResponseAsync(
                    SpdyStream stream,
                    CancellationToken cancellationToken)
                {
                    if (MessageReceived.Count ==
                        Request.Length)
                    {
                        foreach (var response in FragmentedResponses)
                        {
                            Encoding.ASCII.GetBytes(response)
                                .CopyTo(Memory);
                            await stream.SendAsync(
                                    Memory.Slice(0, response.Length),
                                    cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
                        }

                        return true;
                    }

                    return false;
                }

                internal async Task<ReadResult> ReceiveAsync(
                    SpdyStream stream,
                    CancellationToken cancellationToken)
                {
                    var readResult = await stream
                        .ReceiveAsync(
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    MessageReceived
                        .AddRange(
                            readResult.Buffer.Slice(0, readResult.Buffer.Length)
                                .ToArray());
                    return readResult;
                }

                public async ValueTask DisposeAsync()
                {
                    await PortForwardingSocket.DisposeAsync()
                        .ConfigureAwait(false);
                    await KubernetesApiServer.DisposeAsync()
                        .ConfigureAwait(false);
                    _memoryOwner.Dispose();
                }
            }
        }
    }
}