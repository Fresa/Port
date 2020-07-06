﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using k8s;
using Kubernetes.Test.API.Server.Subscriptions.Models;
using Port.Server.IntegrationTests.k8s;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.TestFramework;
using Test.It;
using Xunit;
using Xunit.Abstractions;

namespace Port.Server.IntegrationTests
{
    public partial class Given_a_port_forwarding_endpoint
    {
        public partial class
            When_requesting_to_port_forward : XUnit2ServiceSpecificationAsync<
                PortServerHost>
        {
            private HttpResponseMessage _response;
            private Fixture _fixture;

            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override void Given(
                IServiceContainer configurer)
            {
                _fixture = DisposeAsyncOnTearDown(new Fixture(configurer));
                _fixture.PortForwardingSocket.On<byte[]>(
                    bytes => { _fixture.PortForwardResponseReceived(bytes); });

                _fixture.KubernetesApiServer.Pod.PortForward.OnConnected(
                    new PortForward("test", "service1", 2001), async (
                            socket,
                            cancellationToken) =>
                        await socket.HandleClosing(
                                cancellationToken,
                                async () =>
                                {
                                    await socket.SendPortAsync(
                                            9999, cancellationToken)
                                        .ConfigureAwait(false);

                                    ValueWebSocketReceiveResult readResult;
                                    do
                                    {
                                        readResult = await _fixture
                                            .ReceiveAsync(
                                                socket,
                                                cancellationToken)
                                            .ConfigureAwait(false);

                                        if (readResult.MessageType ==
                                            WebSocketMessageType.Close)
                                        {
                                            return;
                                        }

                                        if (await _fixture
                                            .TrySendResponseAsync(
                                                socket, cancellationToken)
                                            .ConfigureAwait(false))
                                        {
                                            return;
                                        }
                                    } while (readResult.EndOfMessage ==
                                             false ||
                                             cancellationToken
                                                 .IsCancellationRequested ==
                                             false);
                                })
                            .ConfigureAwait(false));
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _response = await Server.CreateHttpClient()
                    .PostAsJsonAsync(
                        "service/test/portforward",
                        new Shared.PortForward
                        {
                            Namespace = "test",
                            Name = "service1",
                            ProtocolType = ProtocolType.Tcp,
                            PodPort = 2001,
                            LocalPort = 1000
                        }, cancellationToken)
                    .ConfigureAwait(false);

                var client =
                    await _fixture.PortForwardingSocket
                        .ConnectAsync(
                            new ByteArrayMessageClientFactory(), IPAddress.Any,
                            1000, ProtocolType.Tcp, cancellationToken);

                await client.SendAsync(
                    Encoding.ASCII.GetBytes(_fixture.Request),
                    cancellationToken);

                await _fixture.WaitForResponseAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            [Fact(
                DisplayName =
                    "k8s api server should receive the request message sent")]
            public void TestReceiveRequestMessage()
            {
                _fixture.WebSocketMessageReceived.Should()
                    .HaveCount(_fixture.Request.Length + 1);
                _fixture.WebSocketMessageReceived[0]
                    .Should()
                    .Be((byte)ChannelIndex.StdIn);
                Encoding.ASCII.GetString(
                        _fixture.WebSocketMessageReceived.GetRange(
                                1, _fixture.Request.Length)
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
                    .Be(_fixture.FragmentedResponses.Aggregate("", (
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
                        Kubernetes.Test.API.Server.TestFramework.Start();
                    container.RegisterSingleton(
                        () => KubernetesApiServer
                            .CreateKubernetesConfiguration());
                }

                internal InMemorySocketTestFramework PortForwardingSocket
                {
                    get;
                }

                internal Kubernetes.Test.API.Server.TestFramework
                    KubernetesApiServer
                { get; }

                internal string Request => @"
POST /cgi-bin/process.cgi HTTP/1.1
User-Agent: Mozilla/4.0 (compatible; MSIE5.01; Windows NT)
Host: www.tutorialspoint.com
Content-Type: text/xml; charset=utf-8
Content-Length: length
Accept-Language: en-us
Accept-Encoding: gzip, deflate
Connection: Keep-Alive

<?xml version=""1.0"" encoding=""utf-8""?>
<string xmlns = ""http://clearforest.com/"">string</string>";

                internal string[] FragmentedResponses => new[]
                {
                    @"
HTTP/1.1 400 Bad Request
Date: Sun, 18 Oct 2012 10:36:20 GMT
Server: Apache/2.2.14 (Win32)
Content-Length: 327
Content-Type: text/html; charset=iso-8859-1
Connection: Closed

<!DOCTYPE HTML PUBLIC ""-//IETF//DTD HTML 2.0//EN"">",
                    @"
<html>
<head>
   <title>400 Bad Request</title>
</head>
<body>
   <h1>Bad Request</h1>
   <p>Your browser sent a request that this server could not understand.</p>
   <p>The request line contained invalid characters following the protocol string.</p>
</body>
</html>"
                };

                private readonly IMemoryOwner<byte> _memoryOwner =
                    MemoryPool<byte>.Shared.Rent(65536);

                internal Memory<byte> Memory => _memoryOwner.Memory;

                internal List<byte> WebSocketMessageReceived =
                    new List<byte>();

                internal string PortForwardResponse { get; private set; }

                internal void PortForwardResponseReceived(
                    byte[] buffer)
                {
                    PortForwardResponse +=
                        Encoding.ASCII.GetString(buffer);
                    _responseReceived.Release();
                }

                private readonly SemaphoreSlim _responseReceived =
                    new SemaphoreSlim(0, 1);


                internal async Task WaitForResponseAsync(
                    CancellationToken cancellationToken)
                {
                    foreach (var _ in FragmentedResponses)
                    {
                        await _responseReceived.WaitAsync(
                                TimeSpan.FromSeconds(5), cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                internal async ValueTask<bool> TrySendResponseAsync(
                    WebSocket webSocket,
                    CancellationToken cancellationToken)
                {
                    if (WebSocketMessageReceived.Count >
                        Request.Length)
                    {
                        foreach (var response in FragmentedResponses)
                        {
                            // Set channel
                            Memory.Span[0] =
                                WebSocketMessageReceived[0];
                            Encoding.ASCII.GetBytes(response)
                                .CopyTo(Memory.Slice(1));
                            await webSocket.SendAsync(
                                    Memory.Slice(
                                        0,
                                        response.Length +
                                        1),
                                    WebSocketMessageType.Binary,
                                    true,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }
                        return true;
                    }

                    return false;
                }

                internal async Task<ValueWebSocketReceiveResult> ReceiveAsync(
                    WebSocket socket,
                    CancellationToken cancellationToken)
                {
                    var readResult = await socket
                        .ReceiveAsync(
                            Memory,
                            cancellationToken)
                        .ConfigureAwait(false);
                    WebSocketMessageReceived
                        .AddRange(
                            Memory.Slice(0, readResult.Count)
                                .ToArray());
                    return readResult;
                }

                public async ValueTask DisposeAsync()
                {
                    await PortForwardingSocket.DisposeAsync();
                    await KubernetesApiServer.DisposeAsync();
                    _memoryOwner.Dispose();
                }
            }
        }
    }
}