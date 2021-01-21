using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Log.It;
using Port.Server.IntegrationTests.TestFramework;
using Port.Server.Kubernetes;
using Port.Server.Spdy.Collections;
using Port.Server.Spdy.Extensions;
using Xunit;
using Xunit.Abstractions;
using Headers = Port.Server.Kubernetes.Headers;
using ReadResult = System.IO.Pipelines.ReadResult;

namespace Port.Server.IntegrationTests
{
    public class UnitTest1 : TestSpecificationAsync
    {
        private ILogger logger = LogFactory.Create<UnitTest1>();

        public UnitTest1(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact(Skip = "Dependent on a real k8s cluster and services")]
        public async Task Test1()
        {
            var factory =
                new KubernetesClientFactory(new KubernetesConfiguration());
            var ks = new KubernetesService(
                factory, new SocketNetworkServerFactory(), new TestFeatureManager());
            await ks.PortForwardAsync(
                        "kind-argo-demo-ci", new Shared.PortForward(
                                podPort: 2746,
                                protocolType: ProtocolType.Tcp,
                                @namespace: "argo",
                                service: "argo-server-5f5c647dcb-bkcz6",
                                pod: "")
                        { LocalPort = 2746 }, CancellationTokenSource.Token)
                    .ConfigureAwait(false);

            await Task.Delay(int.MaxValue)
                      .ConfigureAwait(false);
        }

        [Fact(Skip = "Dependent on a real k8s cluster and services")]
        public async Task TestWithSpdy()
        {
            var config = new KubernetesConfiguration();
            var factory =
                new KubernetesClientFactory(config);
            await using var ks = new KubernetesService(
                factory, new SocketNetworkServerFactory(), new TestFeatureManager((nameof(Features.PortForwardingWithSpdy), true)));
            await ks.PortForwardAsync(
                        "kind-argo-demo-test", new Shared.PortForward(
                                podPort: 80,
                                protocolType: ProtocolType.Tcp,
                                @namespace: "argocd",
                                service: "",
                                pod: "argocd-server-78ffb87fd8-f6559")
                        { LocalPort = 8081 }, CancellationTokenSource.Token)
                    .ConfigureAwait(false);

            await Task.Delay(5000)
                      .ConfigureAwait(false);
        }

        [Fact(Skip = "Dependent on a real k8s cluster and services")]
        //[Fact]
        public async Task TestWithSpdySession()
        {
            var cancellation = new CancellationTokenSource(5000).Token;
            var podPort = 80;
            var config = new KubernetesConfiguration();
            var factory =
                new KubernetesClientFactory(config);
            using var client = factory.Create("kind-argo-demo-test");
            await using var session = await client.SpdyNamespacedPodPortForwardAsync(
                                                      "argocd-server-78ffb87fd8-f6559",
                                                      "argocd",
                                                      new[] { podPort },
                                                      cancellation)
                                                  .ConfigureAwait(false);
            var requestId = "1";

            using var errorStream = session.Open(
                headers: new NameValueHeaderBlock(
                    (Headers.PortForward.StreamType.Key, new[]
                    {
                            Headers.PortForward.StreamType.Error
                    }),
                    (Headers.PortForward.Port, new[]
                    {
                            podPort.ToString()
                    }),
                    (Headers.PortForward.RequestId, new[]
                    {
                            requestId
                    })));

            await errorStream.Remote.WaitForOpenedAsync(cancellation)
                             .ConfigureAwait(false);

            await Task.Delay(1000, cancellation)
                      .ConfigureAwait(false);

            using var stream = session.Open(
                headers: new NameValueHeaderBlock(
                    (Headers.PortForward.StreamType.Key, new[]
                    {
                            Headers.PortForward.StreamType.Data
                    }),
                    (Headers.PortForward.Port, new[]
                    {
                            podPort.ToString()
                    }),
                    (Headers.PortForward.RequestId, new[]
                    {
                            requestId
                    })));

            await stream.Remote.WaitForOpenedAsync(cancellation)
                        .ConfigureAwait(false);

            await stream.SendAsync(
                            new ReadOnlyMemory<byte>(
                                new byte[]
                                {
                                        0x47, 0x45, 0x54, 0x20, 0x2F, 0x20, 0x48, 0x54, 0x54,
                                        0x50, 0x2F, 0x31, 0x2E, 0x31, 0x0D, 0x0A, 0x48, 0x6F,
                                        0x73, 0x74, 0x3A, 0x20, 0x31, 0x32, 0x37, 0x2E, 0x30,
                                        0x2E, 0x30, 0x2E, 0x31, 0x3A, 0x38, 0x30, 0x38, 0x31,
                                        0x0D, 0x0A, 0x43, 0x6F, 0x6E, 0x6E, 0x65, 0x63, 0x74,
                                        0x69, 0x6F, 0x6E, 0x3A, 0x20, 0x6B, 0x65, 0x65, 0x70,
                                        0x2D, 0x61, 0x6C, 0x69, 0x76, 0x65, 0x0D, 0x0A, 0x50,
                                        0x72, 0x61, 0x67, 0x6D, 0x61, 0x3A, 0x20, 0x6E, 0x6F,
                                        0x2D, 0x63, 0x61, 0x63, 0x68, 0x65, 0x0D, 0x0A, 0x43,
                                        0x61, 0x63, 0x68, 0x65, 0x2D, 0x43, 0x6F, 0x6E, 0x74,
                                        0x72, 0x6F, 0x6C, 0x3A, 0x20, 0x6E, 0x6F, 0x2D, 0x63,
                                        0x61, 0x63, 0x68, 0x65, 0x0D, 0x0A, 0x55, 0x70, 0x67,
                                        0x72, 0x61, 0x64, 0x65, 0x2D, 0x49, 0x6E, 0x73, 0x65,
                                        0x63, 0x75, 0x72, 0x65, 0x2D, 0x52, 0x65, 0x71, 0x75,
                                        0x65, 0x73, 0x74, 0x73, 0x3A, 0x20, 0x31, 0x0D, 0x0A,
                                        0x55, 0x73, 0x65, 0x72, 0x2D, 0x41, 0x67, 0x65, 0x6E,
                                        0x74, 0x3A, 0x20, 0x4D, 0x6F, 0x7A, 0x69, 0x6C, 0x6C,
                                        0x61, 0x2F, 0x35, 0x2E, 0x30, 0x20, 0x28, 0x57, 0x69,
                                        0x6E, 0x64, 0x6F, 0x77, 0x73, 0x20, 0x4E, 0x54, 0x20,
                                        0x31, 0x30, 0x2E, 0x30, 0x3B, 0x20, 0x57, 0x69, 0x6E,
                                        0x36, 0x34, 0x3B, 0x20, 0x78, 0x36, 0x34, 0x29, 0x20,
                                        0x41, 0x70, 0x70, 0x6C, 0x65, 0x57, 0x65, 0x62, 0x4B,
                                        0x69, 0x74, 0x2F, 0x35, 0x33, 0x37, 0x2E, 0x33, 0x36,
                                        0x20, 0x28, 0x4B, 0x48, 0x54, 0x4D, 0x4C, 0x2C, 0x20,
                                        0x6C, 0x69, 0x6B, 0x65, 0x20, 0x47, 0x65, 0x63, 0x6B,
                                        0x6F, 0x29, 0x20, 0x43, 0x68, 0x72, 0x6F, 0x6D, 0x65,
                                        0x2F, 0x38, 0x37, 0x2E, 0x30, 0x2E, 0x34, 0x32, 0x38,
                                        0x30, 0x2E, 0x38, 0x38, 0x20, 0x53, 0x61, 0x66, 0x61,
                                        0x72, 0x69, 0x2F, 0x35, 0x33, 0x37, 0x2E, 0x33, 0x36,
                                        0x0D, 0x0A, 0x41, 0x63, 0x63, 0x65, 0x70, 0x74, 0x3A,
                                        0x20, 0x74, 0x65, 0x78, 0x74, 0x2F, 0x68, 0x74, 0x6D,
                                        0x6C, 0x2C, 0x61, 0x70, 0x70, 0x6C, 0x69, 0x63, 0x61,
                                        0x74, 0x69, 0x6F, 0x6E, 0x2F, 0x78, 0x68, 0x74, 0x6D,
                                        0x6C, 0x2B, 0x78, 0x6D, 0x6C, 0x2C, 0x61, 0x70, 0x70,
                                        0x6C, 0x69, 0x63, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x2F,
                                        0x78, 0x6D, 0x6C, 0x3B, 0x71, 0x3D, 0x30, 0x2E, 0x39,
                                        0x2C, 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F, 0x61, 0x76,
                                        0x69, 0x66, 0x2C, 0x69, 0x6D, 0x61, 0x67, 0x65, 0x2F,
                                        0x77, 0x65, 0x62, 0x70, 0x2C, 0x69, 0x6D, 0x61, 0x67,
                                        0x65, 0x2F, 0x61, 0x70, 0x6E, 0x67, 0x2C, 0x2A, 0x2F,
                                        0x2A, 0x3B, 0x71, 0x3D, 0x30, 0x2E, 0x38, 0x2C, 0x61,
                                        0x70, 0x70, 0x6C, 0x69, 0x63, 0x61, 0x74, 0x69, 0x6F,
                                        0x6E, 0x2F, 0x73, 0x69, 0x67, 0x6E, 0x65, 0x64, 0x2D,
                                        0x65, 0x78, 0x63, 0x68, 0x61, 0x6E, 0x67, 0x65, 0x3B,
                                        0x76, 0x3D, 0x62, 0x33, 0x3B, 0x71, 0x3D, 0x30, 0x2E,
                                        0x39, 0x0D, 0x0A, 0x53, 0x65, 0x63, 0x2D, 0x46, 0x65,
                                        0x74, 0x63, 0x68, 0x2D, 0x53, 0x69, 0x74, 0x65, 0x3A,
                                        0x20, 0x6E, 0x6F, 0x6E, 0x65, 0x0D, 0x0A, 0x53, 0x65,
                                        0x63, 0x2D, 0x46, 0x65, 0x74, 0x63, 0x68, 0x2D, 0x4D,
                                        0x6F, 0x64, 0x65, 0x3A, 0x20, 0x6E, 0x61, 0x76, 0x69,
                                        0x67, 0x61, 0x74, 0x65, 0x0D, 0x0A, 0x53, 0x65, 0x63,
                                        0x2D, 0x46, 0x65, 0x74, 0x63, 0x68, 0x2D, 0x55, 0x73,
                                        0x65, 0x72, 0x3A, 0x20, 0x3F, 0x31, 0x0D, 0x0A, 0x53,
                                        0x65, 0x63, 0x2D, 0x46, 0x65, 0x74, 0x63, 0x68, 0x2D,
                                        0x44, 0x65, 0x73, 0x74, 0x3A, 0x20, 0x64, 0x6F, 0x63,
                                        0x75, 0x6D, 0x65, 0x6E, 0x74, 0x0D, 0x0A, 0x41, 0x63,
                                        0x63, 0x65, 0x70, 0x74, 0x2D, 0x45, 0x6E, 0x63, 0x6F,
                                        0x64, 0x69, 0x6E, 0x67, 0x3A, 0x20, 0x67, 0x7A, 0x69,
                                        0x70, 0x2C, 0x20, 0x64, 0x65, 0x66, 0x6C, 0x61, 0x74,
                                        0x65, 0x2C, 0x20, 0x62, 0x72, 0x0D, 0x0A, 0x41, 0x63,
                                        0x63, 0x65, 0x70, 0x74, 0x2D, 0x4C, 0x61, 0x6E, 0x67,
                                        0x75, 0x61, 0x67, 0x65, 0x3A, 0x20, 0x65, 0x6E, 0x2D,
                                        0x53, 0x45, 0x2C, 0x65, 0x6E, 0x3B, 0x71, 0x3D, 0x30,
                                        0x2E, 0x39, 0x2C, 0x73, 0x76, 0x2D, 0x53, 0x45, 0x3B,
                                        0x71, 0x3D, 0x30, 0x2E, 0x38, 0x2C, 0x73, 0x76, 0x3B,
                                        0x71, 0x3D, 0x30, 0x2E, 0x37, 0x2C, 0x65, 0x6E, 0x2D,
                                        0x47, 0x42, 0x3B, 0x71, 0x3D, 0x30, 0x2E, 0x36, 0x2C,
                                        0x65, 0x6E, 0x2D, 0x55, 0x53, 0x3B, 0x71, 0x3D, 0x30,
                                        0x2E, 0x35, 0x0D, 0x0A, 0x0D, 0x0A
                                }), cancellationToken: cancellation)
                        .ConfigureAwait(false);

            ReadResult result;
            do
            {
                result = await stream
                               .ReceiveAsync(timeout: TimeSpan.FromSeconds(2))
                               .ConfigureAwait(false);

                var buffer = result.Buffer.Slice(
                    0, result.Buffer.Length);
                logger.Debug(
                    buffer
                        .ToArray()
                        .ToHexArrayRepresentation());
            } while (result.HasMoreData());

            await Task.Delay(2000, cancellation)
                      .ConfigureAwait(false);
        }
    }
}
