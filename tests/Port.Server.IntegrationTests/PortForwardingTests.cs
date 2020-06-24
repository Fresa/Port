using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Port.Server.IntegrationTests.SocketTestFramework;
using Port.Server.IntegrationTests.TestFramework;
using Port.Shared;
using Test.It;
using Xunit;
using Xunit.Abstractions;
using PortForward = Kubernetes.Test.API.Server.Subscriptions.Models.PortForward;

namespace Port.Server.IntegrationTests
{
    public class Given_a_port_forwarding_endpoint
    {
        public partial class
            When_requesting_to_port_forward : XUnit2ServiceSpecificationAsync<
                PortServerHost>
        {
            private Kubernetes.Test.API.Server.TestFramework _k8sApiServer;
            private HttpResponseMessage _response;
            private InMemorySocketTestFramework _portforwardingSocketTestFramework;
            private PortForward _portforwardRequested;

            public When_requesting_to_port_forward(
                ITestOutputHelper testOutputHelper)
                : base(testOutputHelper)
            {
            }

            protected override void Given(
                IServiceContainer configurer)
            {
                _portforwardingSocketTestFramework =
                    DisposeAsyncOnTearDown(
                        SocketTestFramework.SocketTestFramework.InMemory());
                _portforwardingSocketTestFramework.On<byte[]>(
                    respond =>
                    {

                    });
                configurer.RegisterSingleton(
                    () => _portforwardingSocketTestFramework.NetworkServerFactory);

                _k8sApiServer =
                    DisposeAsyncOnTearDown(
                        Kubernetes.Test.API.Server.TestFramework.Start());
                _k8sApiServer.PodSubscriptions.OnPortForward(
                    forward =>
                    {
                        _portforwardRequested = forward;
                        return Task.FromResult(new ActionResult<string>(""));
                    });
                configurer.RegisterSingleton(
                    () => _k8sApiServer.CreateKubernetesConfiguration());
            }

            protected override async Task WhenAsync(
                CancellationToken cancellationToken)
            {
                _response = await Server.CreateHttpClient()
                    .PostAsJsonAsync(
                        "service/kind-argo-demo-ci/portforward",
                        new Shared.PortForward
                        {
                            Namespace = "test",
                            Name = "service1",
                            ProtocolType = ProtocolType.Tcp,
                            From = 2001,
                            To = 1000
                        }, cancellationToken)
                    .ConfigureAwait(false);

                var client = await _portforwardingSocketTestFramework.ConnectAsync(
                    new ByteArrayMessageClientFactory(), IPAddress.Any, 1000, ProtocolType.Tcp, cancellationToken);
            }

            [Fact]
            public async Task
                It_should_establish_a_web_socket_connection_to_k8s_api_server()
            {
                _response.StatusCode.Should()
                    .Be(HttpStatusCode.OK);
            }
        }
    }

    internal sealed class ByteArrayMessageClientFactory : IMessageClientFactory<byte[]>
    {
        public IMessageClient<byte[]> Create(
            INetworkClient networkClient)
        {
            return new ByteArrayMessageClient(networkClient);
        }
    }

    internal sealed class ByteArrayMessageClient : IMessageClient<byte[]>
    {
        private readonly INetworkClient _networkClient;

        public ByteArrayMessageClient(INetworkClient networkClient)
        {
            _networkClient = networkClient;
        }

        public async ValueTask<byte[]> ReceiveAsync(
            CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(65536);
            var memory = memoryOwner.Memory;

            await _networkClient.ReceiveAsync(memory, cancellationToken);
            return memory.ToArray();
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public async ValueTask SendAsync(
            byte[] payload,
            CancellationToken cancellationToken = default)
        {
            await _networkClient.SendAsync(payload, cancellationToken);
        }
    }

    internal sealed class HttpResponseMessageClient :
        IReceivingClient<HttpResponse>, ISendingClient<HttpResponse>
    {
        private readonly INetworkClient _networkClient;

        public HttpResponseMessageClient(
            INetworkClient networkClient)
        {
            _networkClient = networkClient;
        }

        public async ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public bool CanHandle(
            Type type)
        {
            return type == typeof(HttpResponse);
        }

        public async ValueTask<HttpResponse> ReceiveAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask SendAsync(
            HttpResponse payload,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class HttpResponse
    {
        /// <summary>
        /// Gets or sets the HTTP response code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets the response headers.
        /// </summary>
        public IHeaderDictionary Headers { get; } = new HeaderDictionary();

        /// <summary>
        /// Gets or sets the response body <see cref="Stream"/>.
        /// </summary>
        public Stream Body { get; set; }
    }
}