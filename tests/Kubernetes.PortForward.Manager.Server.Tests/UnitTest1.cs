using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Test.It.Specifications;
using Xunit;
using Xunit.Abstractions;

namespace Kubernetes.PortForward.Manager.Server.Tests
{
    public class UnitTest1 : TestSpecificationAsync
    {

        public UnitTest1(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Test1()
        {
            var factory = new KubernetesClientFactory();
            var ks = new KubernetesService(factory);
            await ks.PortForwardAsync(
                "kind-argo-demo-ci", new Shared.PortForward
                {
                    To = 2746,
                    From = 2746,
                    ProtocolType = ProtocolType.Tcp,
                    Namespace = "argo",
                    Name = "argo-server-7495b6b74b-4rqrg"
                }).ConfigureAwait(false);

            await Task.Delay(int.MaxValue);
        }

    }

    internal sealed class MyKubernetesClientFactory : IKubernetesClientFactory
    {
        public IKubernetes Create(
            string context)
        {
            return new MyKubernetes();
        }
    }

    public class MyKubernetes : ServiceClient<MyKubernetes>, IKubernetes
    {
        public Uri BaseUri { get; set; }

        public JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

        public JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

        public ServiceClientCredentials Credentials { get; set; }

        public Task<HttpOperationResponse<string>> ConnectDeleteNamespacedPodProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectDeleteNamespacedPodProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectDeleteNamespacedServiceProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectDeleteNamespacedServiceProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectDeleteNodeProxyWithHttpMessagesAsync(string name, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectDeleteNodeProxyWithPathWithHttpMessagesAsync(string name, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNamespacedPodAttachWithHttpMessagesAsync(string name, string namespaceParameter, string container = null, bool? stderr = null, bool? stdin = null, bool? stdout = null, bool? tty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNamespacedPodExecWithHttpMessagesAsync(string name, string namespaceParameter, string command = null, string container = null, bool? stderr = null, bool? stdin = null, bool? stdout = null, bool? tty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNamespacedPodPortforwardWithHttpMessagesAsync(string name, string namespaceParameter, int? ports = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNamespacedPodProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNamespacedPodProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNamespacedServiceProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNamespacedServiceProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNodeProxyWithHttpMessagesAsync(string name, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectGetNodeProxyWithPathWithHttpMessagesAsync(string name, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectHeadNamespacedPodProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectHeadNamespacedPodProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectHeadNamespacedServiceProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectHeadNamespacedServiceProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectHeadNodeProxyWithHttpMessagesAsync(string name, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectHeadNodeProxyWithPathWithHttpMessagesAsync(string name, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPatchNamespacedPodProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPatchNamespacedPodProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPatchNamespacedServiceProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPatchNamespacedServiceProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPatchNodeProxyWithHttpMessagesAsync(string name, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPatchNodeProxyWithPathWithHttpMessagesAsync(string name, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNamespacedPodAttachWithHttpMessagesAsync(string name, string namespaceParameter, string container = null, bool? stderr = null, bool? stdin = null, bool? stdout = null, bool? tty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNamespacedPodExecWithHttpMessagesAsync(string name, string namespaceParameter, string command = null, string container = null, bool? stderr = null, bool? stdin = null, bool? stdout = null, bool? tty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNamespacedPodPortforwardWithHttpMessagesAsync(string name, string namespaceParameter, int? ports = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNamespacedPodProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNamespacedPodProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNamespacedServiceProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNamespacedServiceProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNodeProxyWithHttpMessagesAsync(string name, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPostNodeProxyWithPathWithHttpMessagesAsync(string name, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPutNamespacedPodProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPutNamespacedPodProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPutNamespacedServiceProxyWithHttpMessagesAsync(string name, string namespaceParameter, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPutNamespacedServiceProxyWithPathWithHttpMessagesAsync(string name, string namespaceParameter, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPutNodeProxyWithHttpMessagesAsync(string name, string path = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> ConnectPutNodeProxyWithPathWithHttpMessagesAsync(string name, string path, string path1, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIService>> CreateAPIService1WithHttpMessagesAsync(V1beta1APIService body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIService>> CreateAPIServiceWithHttpMessagesAsync(V1APIService body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1AuditSink>> CreateAuditSinkWithHttpMessagesAsync(V1alpha1AuditSink body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> CreateCertificateSigningRequestWithHttpMessagesAsync(V1beta1CertificateSigningRequest body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> CreateClusterCustomObjectWithHttpMessagesAsync(object body, string group, string version, string plural, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRole>> CreateClusterRole1WithHttpMessagesAsync(V1alpha1ClusterRole body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRole>> CreateClusterRole2WithHttpMessagesAsync(V1beta1ClusterRole body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRoleBinding>> CreateClusterRoleBinding1WithHttpMessagesAsync(V1alpha1ClusterRoleBinding body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRoleBinding>> CreateClusterRoleBinding2WithHttpMessagesAsync(V1beta1ClusterRoleBinding body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRoleBinding>> CreateClusterRoleBindingWithHttpMessagesAsync(V1ClusterRoleBinding body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRole>> CreateClusterRoleWithHttpMessagesAsync(V1ClusterRole body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSIDriver>> CreateCSIDriver1WithHttpMessagesAsync(V1beta1CSIDriver body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSIDriver>> CreateCSIDriverWithHttpMessagesAsync(V1CSIDriver body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSINode>> CreateCSINode1WithHttpMessagesAsync(V1beta1CSINode body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSINode>> CreateCSINodeWithHttpMessagesAsync(V1CSINode body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinition>> CreateCustomResourceDefinition1WithHttpMessagesAsync(V1beta1CustomResourceDefinition body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinition>> CreateCustomResourceDefinitionWithHttpMessagesAsync(V1CustomResourceDefinition body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchema>> CreateFlowSchemaWithHttpMessagesAsync(V1alpha1FlowSchema body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1IngressClass>> CreateIngressClassWithHttpMessagesAsync(V1beta1IngressClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1MutatingWebhookConfiguration>> CreateMutatingWebhookConfiguration1WithHttpMessagesAsync(V1beta1MutatingWebhookConfiguration body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1MutatingWebhookConfiguration>> CreateMutatingWebhookConfigurationWithHttpMessagesAsync(V1MutatingWebhookConfiguration body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Binding>> CreateNamespacedBindingWithHttpMessagesAsync(V1Binding body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ConfigMap>> CreateNamespacedConfigMapWithHttpMessagesAsync(V1ConfigMap body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ControllerRevision>> CreateNamespacedControllerRevisionWithHttpMessagesAsync(V1ControllerRevision body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJob>> CreateNamespacedCronJob1WithHttpMessagesAsync(V2alpha1CronJob body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJob>> CreateNamespacedCronJobWithHttpMessagesAsync(V1beta1CronJob body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> CreateNamespacedCustomObjectWithHttpMessagesAsync(object body, string group, string version, string namespaceParameter, string plural, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSet>> CreateNamespacedDaemonSetWithHttpMessagesAsync(V1DaemonSet body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Deployment>> CreateNamespacedDeploymentWithHttpMessagesAsync(V1Deployment body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EndpointSlice>> CreateNamespacedEndpointSliceWithHttpMessagesAsync(V1beta1EndpointSlice body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Endpoints>> CreateNamespacedEndpointsWithHttpMessagesAsync(V1Endpoints body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Event>> CreateNamespacedEvent1WithHttpMessagesAsync(V1beta1Event body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Event>> CreateNamespacedEventWithHttpMessagesAsync(V1Event body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscaler>> CreateNamespacedHorizontalPodAutoscaler1WithHttpMessagesAsync(V2beta1HorizontalPodAutoscaler body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscaler>> CreateNamespacedHorizontalPodAutoscaler2WithHttpMessagesAsync(V2beta2HorizontalPodAutoscaler body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscaler>> CreateNamespacedHorizontalPodAutoscalerWithHttpMessagesAsync(V1HorizontalPodAutoscaler body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1Ingress>> CreateNamespacedIngress1WithHttpMessagesAsync(Networkingv1beta1Ingress body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1Ingress>> CreateNamespacedIngressWithHttpMessagesAsync(Extensionsv1beta1Ingress body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Job>> CreateNamespacedJobWithHttpMessagesAsync(V1Job body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Lease>> CreateNamespacedLease1WithHttpMessagesAsync(V1beta1Lease body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Lease>> CreateNamespacedLeaseWithHttpMessagesAsync(V1Lease body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LimitRange>> CreateNamespacedLimitRangeWithHttpMessagesAsync(V1LimitRange body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1LocalSubjectAccessReview>> CreateNamespacedLocalSubjectAccessReview1WithHttpMessagesAsync(V1beta1LocalSubjectAccessReview body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LocalSubjectAccessReview>> CreateNamespacedLocalSubjectAccessReviewWithHttpMessagesAsync(V1LocalSubjectAccessReview body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NetworkPolicy>> CreateNamespacedNetworkPolicyWithHttpMessagesAsync(V1NetworkPolicy body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> CreateNamespacedPersistentVolumeClaimWithHttpMessagesAsync(V1PersistentVolumeClaim body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Binding>> CreateNamespacedPodBindingWithHttpMessagesAsync(V1Binding body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudget>> CreateNamespacedPodDisruptionBudgetWithHttpMessagesAsync(V1beta1PodDisruptionBudget body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Eviction>> CreateNamespacedPodEvictionWithHttpMessagesAsync(V1beta1Eviction body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PodPreset>> CreateNamespacedPodPresetWithHttpMessagesAsync(V1alpha1PodPreset body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodTemplate>> CreateNamespacedPodTemplateWithHttpMessagesAsync(V1PodTemplate body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> CreateNamespacedPodWithHttpMessagesAsync(V1Pod body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSet>> CreateNamespacedReplicaSetWithHttpMessagesAsync(V1ReplicaSet body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationController>> CreateNamespacedReplicationControllerWithHttpMessagesAsync(V1ReplicationController body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> CreateNamespacedResourceQuotaWithHttpMessagesAsync(V1ResourceQuota body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1Role>> CreateNamespacedRole1WithHttpMessagesAsync(V1alpha1Role body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Role>> CreateNamespacedRole2WithHttpMessagesAsync(V1beta1Role body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleBinding>> CreateNamespacedRoleBinding1WithHttpMessagesAsync(V1alpha1RoleBinding body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleBinding>> CreateNamespacedRoleBinding2WithHttpMessagesAsync(V1beta1RoleBinding body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleBinding>> CreateNamespacedRoleBindingWithHttpMessagesAsync(V1RoleBinding body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Role>> CreateNamespacedRoleWithHttpMessagesAsync(V1Role body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Secret>> CreateNamespacedSecretWithHttpMessagesAsync(V1Secret body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1TokenRequest>> CreateNamespacedServiceAccountTokenWithHttpMessagesAsync(V1TokenRequest body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceAccount>> CreateNamespacedServiceAccountWithHttpMessagesAsync(V1ServiceAccount body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Service>> CreateNamespacedServiceWithHttpMessagesAsync(V1Service body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSet>> CreateNamespacedStatefulSetWithHttpMessagesAsync(V1StatefulSet body, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> CreateNamespaceWithHttpMessagesAsync(V1Namespace body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Node>> CreateNodeWithHttpMessagesAsync(V1Node body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> CreatePersistentVolumeWithHttpMessagesAsync(V1PersistentVolume body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodSecurityPolicy>> CreatePodSecurityPolicyWithHttpMessagesAsync(V1beta1PodSecurityPolicy body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityClass>> CreatePriorityClass1WithHttpMessagesAsync(V1alpha1PriorityClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PriorityClass>> CreatePriorityClass2WithHttpMessagesAsync(V1beta1PriorityClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PriorityClass>> CreatePriorityClassWithHttpMessagesAsync(V1PriorityClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfiguration>> CreatePriorityLevelConfigurationWithHttpMessagesAsync(V1alpha1PriorityLevelConfiguration body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RuntimeClass>> CreateRuntimeClass1WithHttpMessagesAsync(V1beta1RuntimeClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RuntimeClass>> CreateRuntimeClassWithHttpMessagesAsync(V1alpha1RuntimeClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1SelfSubjectAccessReview>> CreateSelfSubjectAccessReview1WithHttpMessagesAsync(V1beta1SelfSubjectAccessReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1SelfSubjectAccessReview>> CreateSelfSubjectAccessReviewWithHttpMessagesAsync(V1SelfSubjectAccessReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1SelfSubjectRulesReview>> CreateSelfSubjectRulesReview1WithHttpMessagesAsync(V1beta1SelfSubjectRulesReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1SelfSubjectRulesReview>> CreateSelfSubjectRulesReviewWithHttpMessagesAsync(V1SelfSubjectRulesReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1StorageClass>> CreateStorageClass1WithHttpMessagesAsync(V1beta1StorageClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StorageClass>> CreateStorageClassWithHttpMessagesAsync(V1StorageClass body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1SubjectAccessReview>> CreateSubjectAccessReview1WithHttpMessagesAsync(V1beta1SubjectAccessReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1SubjectAccessReview>> CreateSubjectAccessReviewWithHttpMessagesAsync(V1SubjectAccessReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1TokenReview>> CreateTokenReview1WithHttpMessagesAsync(V1beta1TokenReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1TokenReview>> CreateTokenReviewWithHttpMessagesAsync(V1TokenReview body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ValidatingWebhookConfiguration>> CreateValidatingWebhookConfiguration1WithHttpMessagesAsync(V1beta1ValidatingWebhookConfiguration body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ValidatingWebhookConfiguration>> CreateValidatingWebhookConfigurationWithHttpMessagesAsync(V1ValidatingWebhookConfiguration body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1VolumeAttachment>> CreateVolumeAttachment1WithHttpMessagesAsync(V1alpha1VolumeAttachment body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1VolumeAttachment>> CreateVolumeAttachment2WithHttpMessagesAsync(V1beta1VolumeAttachment body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> CreateVolumeAttachmentWithHttpMessagesAsync(V1VolumeAttachment body, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteAPIService1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteAPIServiceWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteAuditSinkWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCertificateSigningRequestWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> DeleteClusterCustomObjectWithHttpMessagesAsync(string group, string version, string plural, string name, V1DeleteOptions body = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteClusterRole1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteClusterRole2WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteClusterRoleBinding1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteClusterRoleBinding2WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteClusterRoleBindingWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteClusterRoleWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionAPIService1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionAPIServiceWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionAuditSinkWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionCertificateSigningRequestWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionClusterRole1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionClusterRole2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionClusterRoleBinding1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionClusterRoleBinding2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionClusterRoleBindingWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionClusterRoleWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionCSIDriver1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionCSIDriverWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionCSINode1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionCSINodeWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionCustomResourceDefinition1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionCustomResourceDefinitionWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionFlowSchemaWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionIngressClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionMutatingWebhookConfiguration1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionMutatingWebhookConfigurationWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedConfigMapWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedControllerRevisionWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedCronJob1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedCronJobWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedDaemonSetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedDeploymentWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedEndpointSliceWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedEndpointsWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedEvent1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedEventWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedHorizontalPodAutoscaler1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedHorizontalPodAutoscaler2WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedHorizontalPodAutoscalerWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedIngress1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedIngressWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedJobWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedLease1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedLeaseWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedLimitRangeWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedNetworkPolicyWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedPersistentVolumeClaimWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedPodDisruptionBudgetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedPodPresetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedPodTemplateWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedPodWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedReplicaSetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedReplicationControllerWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedResourceQuotaWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedRole1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedRole2WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedRoleBinding1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedRoleBinding2WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedRoleBindingWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedRoleWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedSecretWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedServiceAccountWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNamespacedStatefulSetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionNodeWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionPersistentVolumeWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionPodSecurityPolicyWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionPriorityClass1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionPriorityClass2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionPriorityClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionPriorityLevelConfigurationWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionRuntimeClass1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionRuntimeClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionStorageClass1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionStorageClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionValidatingWebhookConfiguration1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionValidatingWebhookConfigurationWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionVolumeAttachment1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionVolumeAttachment2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCollectionVolumeAttachmentWithHttpMessagesAsync(bool? allowWatchBookmarks = null, V1DeleteOptions body = null, string continueParameter = null, string dryRun = null, string fieldSelector = null, int? gracePeriodSeconds = null, string labelSelector = null, int? limit = null, bool? orphanDependents = null, string propagationPolicy = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSIDriver>> DeleteCSIDriver1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSIDriver>> DeleteCSIDriverWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSINode>> DeleteCSINode1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSINode>> DeleteCSINodeWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCustomResourceDefinition1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteCustomResourceDefinitionWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteFlowSchemaWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteIngressClassWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteMutatingWebhookConfiguration1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteMutatingWebhookConfigurationWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedConfigMapWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedControllerRevisionWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedCronJob1WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedCronJobWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> DeleteNamespacedCustomObjectWithHttpMessagesAsync(string group, string version, string namespaceParameter, string plural, string name, V1DeleteOptions body = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedDaemonSetWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedDeploymentWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedEndpointSliceWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedEndpointsWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedEvent1WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedEventWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedHorizontalPodAutoscaler1WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedHorizontalPodAutoscaler2WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedHorizontalPodAutoscalerWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedIngress1WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedIngressWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedJobWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedLease1WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedLeaseWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedLimitRangeWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedNetworkPolicyWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> DeleteNamespacedPersistentVolumeClaimWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedPodDisruptionBudgetWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedPodPresetWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodTemplate>> DeleteNamespacedPodTemplateWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> DeleteNamespacedPodWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedReplicaSetWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedReplicationControllerWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> DeleteNamespacedResourceQuotaWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedRole1WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedRole2WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedRoleBinding1WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedRoleBinding2WithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedRoleBindingWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedRoleWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedSecretWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceAccount>> DeleteNamespacedServiceAccountWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedServiceWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespacedStatefulSetWithHttpMessagesAsync(string name, string namespaceParameter, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNamespaceWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteNodeWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> DeletePersistentVolumeWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodSecurityPolicy>> DeletePodSecurityPolicyWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeletePriorityClass1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeletePriorityClass2WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeletePriorityClassWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeletePriorityLevelConfigurationWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteRuntimeClass1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteRuntimeClassWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1StorageClass>> DeleteStorageClass1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StorageClass>> DeleteStorageClassWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteValidatingWebhookConfiguration1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Status>> DeleteValidatingWebhookConfigurationWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1VolumeAttachment>> DeleteVolumeAttachment1WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1VolumeAttachment>> DeleteVolumeAttachment2WithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> DeleteVolumeAttachmentWithHttpMessagesAsync(string name, V1DeleteOptions body = null, string dryRun = null, int? gracePeriodSeconds = null, bool? orphanDependents = null, string propagationPolicy = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup10WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup11WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup12WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup13WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup14WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup15WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup16WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup17WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup18WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup19WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup1WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup20WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup21WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup2WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup3WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup4WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup5WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup6WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup7WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup8WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroup9WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroup>> GetAPIGroupWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources10WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources11WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources12WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources13WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources14WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources15WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources16WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources17WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources18WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources19WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources1WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources20WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources21WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources22WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources23WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources24WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources25WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources26WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources27WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources28WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources29WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources2WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources30WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources31WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources32WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources33WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources34WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources35WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources36WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources37WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources38WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources39WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources3WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources40WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources4WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources5WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources6WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources7WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources8WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResources9WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIResourceList>> GetAPIResourcesWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIGroupList>> GetAPIVersions1WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIVersions>> GetAPIVersionsWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> GetClusterCustomObjectScaleWithHttpMessagesAsync(string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> GetClusterCustomObjectStatusWithHttpMessagesAsync(string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> GetClusterCustomObjectWithHttpMessagesAsync(string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<VersionInfo>> GetCodeWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> GetNamespacedCustomObjectScaleWithHttpMessagesAsync(string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> GetNamespacedCustomObjectStatusWithHttpMessagesAsync(string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> GetNamespacedCustomObjectWithHttpMessagesAsync(string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIServiceList>> ListAPIService1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIServiceList>> ListAPIServiceWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1AuditSinkList>> ListAuditSinkWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequestList>> ListCertificateSigningRequestWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ListClusterCustomObjectWithHttpMessagesAsync(string group, string version, string plural, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRoleList>> ListClusterRole1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRoleList>> ListClusterRole2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRoleBindingList>> ListClusterRoleBinding1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRoleBindingList>> ListClusterRoleBinding2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRoleBindingList>> ListClusterRoleBindingWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRoleList>> ListClusterRoleWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ComponentStatusList>> ListComponentStatusWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ConfigMapList>> ListConfigMapForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ControllerRevisionList>> ListControllerRevisionForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJobList>> ListCronJobForAllNamespaces1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJobList>> ListCronJobForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSIDriverList>> ListCSIDriver1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSIDriverList>> ListCSIDriverWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSINodeList>> ListCSINode1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSINodeList>> ListCSINodeWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinitionList>> ListCustomResourceDefinition1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinitionList>> ListCustomResourceDefinitionWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSetList>> ListDaemonSetForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DeploymentList>> ListDeploymentForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1EndpointsList>> ListEndpointsForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EndpointSliceList>> ListEndpointSliceForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EventList>> ListEventForAllNamespaces1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1EventList>> ListEventForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchemaList>> ListFlowSchemaWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscalerList>> ListHorizontalPodAutoscalerForAllNamespaces1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscalerList>> ListHorizontalPodAutoscalerForAllNamespaces2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscalerList>> ListHorizontalPodAutoscalerForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1IngressClassList>> ListIngressClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1IngressList>> ListIngressForAllNamespaces1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1IngressList>> ListIngressForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1JobList>> ListJobForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1LeaseList>> ListLeaseForAllNamespaces1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LeaseList>> ListLeaseForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LimitRangeList>> ListLimitRangeForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1MutatingWebhookConfigurationList>> ListMutatingWebhookConfiguration1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1MutatingWebhookConfigurationList>> ListMutatingWebhookConfigurationWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ConfigMapList>> ListNamespacedConfigMapWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ControllerRevisionList>> ListNamespacedControllerRevisionWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJobList>> ListNamespacedCronJob1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJobList>> ListNamespacedCronJobWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ListNamespacedCustomObjectWithHttpMessagesAsync(string group, string version, string namespaceParameter, string plural, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSetList>> ListNamespacedDaemonSetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DeploymentList>> ListNamespacedDeploymentWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EndpointSliceList>> ListNamespacedEndpointSliceWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1EndpointsList>> ListNamespacedEndpointsWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EventList>> ListNamespacedEvent1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1EventList>> ListNamespacedEventWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscalerList>> ListNamespacedHorizontalPodAutoscaler1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscalerList>> ListNamespacedHorizontalPodAutoscaler2WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscalerList>> ListNamespacedHorizontalPodAutoscalerWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1IngressList>> ListNamespacedIngress1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1IngressList>> ListNamespacedIngressWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1JobList>> ListNamespacedJobWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1LeaseList>> ListNamespacedLease1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LeaseList>> ListNamespacedLeaseWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LimitRangeList>> ListNamespacedLimitRangeWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NetworkPolicyList>> ListNamespacedNetworkPolicyWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaimList>> ListNamespacedPersistentVolumeClaimWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudgetList>> ListNamespacedPodDisruptionBudgetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PodPresetList>> ListNamespacedPodPresetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodTemplateList>> ListNamespacedPodTemplateWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodList>> ListNamespacedPodWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSetList>> ListNamespacedReplicaSetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationControllerList>> ListNamespacedReplicationControllerWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuotaList>> ListNamespacedResourceQuotaWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleList>> ListNamespacedRole1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleList>> ListNamespacedRole2WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleBindingList>> ListNamespacedRoleBinding1WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleBindingList>> ListNamespacedRoleBinding2WithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleBindingList>> ListNamespacedRoleBindingWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleList>> ListNamespacedRoleWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1SecretList>> ListNamespacedSecretWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceAccountList>> ListNamespacedServiceAccountWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceList>> ListNamespacedServiceWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSetList>> ListNamespacedStatefulSetWithHttpMessagesAsync(string namespaceParameter, bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NamespaceList>> ListNamespaceWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NetworkPolicyList>> ListNetworkPolicyForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NodeList>> ListNodeWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaimList>> ListPersistentVolumeClaimForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeList>> ListPersistentVolumeWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudgetList>> ListPodDisruptionBudgetForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodList>> ListPodForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PodPresetList>> ListPodPresetForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodSecurityPolicyList>> ListPodSecurityPolicyWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodTemplateList>> ListPodTemplateForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityClassList>> ListPriorityClass1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PriorityClassList>> ListPriorityClass2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PriorityClassList>> ListPriorityClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfigurationList>> ListPriorityLevelConfigurationWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSetList>> ListReplicaSetForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationControllerList>> ListReplicationControllerForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuotaList>> ListResourceQuotaForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleBindingList>> ListRoleBindingForAllNamespaces1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleBindingList>> ListRoleBindingForAllNamespaces2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleBindingList>> ListRoleBindingForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleList>> ListRoleForAllNamespaces1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleList>> ListRoleForAllNamespaces2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleList>> ListRoleForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RuntimeClassList>> ListRuntimeClass1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RuntimeClassList>> ListRuntimeClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1SecretList>> ListSecretForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceAccountList>> ListServiceAccountForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceList>> ListServiceForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSetList>> ListStatefulSetForAllNamespacesWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1StorageClassList>> ListStorageClass1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StorageClassList>> ListStorageClassWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ValidatingWebhookConfigurationList>> ListValidatingWebhookConfiguration1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ValidatingWebhookConfigurationList>> ListValidatingWebhookConfigurationWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1VolumeAttachmentList>> ListVolumeAttachment1WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1VolumeAttachmentList>> ListVolumeAttachment2WithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachmentList>> ListVolumeAttachmentWithHttpMessagesAsync(bool? allowWatchBookmarks = null, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> LogFileHandlerWithHttpMessagesAsync(string logpath, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> LogFileListHandlerWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IStreamDemuxer> MuxedStreamNamespacedPodExecAsync(string name, string @namespace = "default", IEnumerable<string> command = null, string container = null, bool stderr = true, bool stdin = true, bool stdout = true, bool tty = true, string webSocketSubProtol = "v4.channel.k8s.io", Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> NamespacedPodExecAsync(string name, string @namespace, string container, IEnumerable<string> command, bool tty, ExecAsyncCallback action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIService>> PatchAPIService1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIService>> PatchAPIServiceStatus1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIService>> PatchAPIServiceStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIService>> PatchAPIServiceWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1AuditSink>> PatchAuditSinkWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> PatchCertificateSigningRequestStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> PatchCertificateSigningRequestWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> PatchClusterCustomObjectScaleWithHttpMessagesAsync(object body, string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> PatchClusterCustomObjectStatusWithHttpMessagesAsync(object body, string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> PatchClusterCustomObjectWithHttpMessagesAsync(object body, string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRole>> PatchClusterRole1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRole>> PatchClusterRole2WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRoleBinding>> PatchClusterRoleBinding1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRoleBinding>> PatchClusterRoleBinding2WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRoleBinding>> PatchClusterRoleBindingWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRole>> PatchClusterRoleWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSIDriver>> PatchCSIDriver1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSIDriver>> PatchCSIDriverWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSINode>> PatchCSINode1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSINode>> PatchCSINodeWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinition>> PatchCustomResourceDefinition1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinition>> PatchCustomResourceDefinitionStatus1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinition>> PatchCustomResourceDefinitionStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinition>> PatchCustomResourceDefinitionWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchema>> PatchFlowSchemaStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchema>> PatchFlowSchemaWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1IngressClass>> PatchIngressClassWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1MutatingWebhookConfiguration>> PatchMutatingWebhookConfiguration1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1MutatingWebhookConfiguration>> PatchMutatingWebhookConfigurationWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ConfigMap>> PatchNamespacedConfigMapWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ControllerRevision>> PatchNamespacedControllerRevisionWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJob>> PatchNamespacedCronJob1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJob>> PatchNamespacedCronJobStatus1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJob>> PatchNamespacedCronJobStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJob>> PatchNamespacedCronJobWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> PatchNamespacedCustomObjectScaleWithHttpMessagesAsync(object body, string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> PatchNamespacedCustomObjectStatusWithHttpMessagesAsync(object body, string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> PatchNamespacedCustomObjectWithHttpMessagesAsync(object body, string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSet>> PatchNamespacedDaemonSetStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSet>> PatchNamespacedDaemonSetWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> PatchNamespacedDeploymentScaleWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Deployment>> PatchNamespacedDeploymentStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Deployment>> PatchNamespacedDeploymentWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EndpointSlice>> PatchNamespacedEndpointSliceWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Endpoints>> PatchNamespacedEndpointsWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Event>> PatchNamespacedEvent1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Event>> PatchNamespacedEventWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscaler>> PatchNamespacedHorizontalPodAutoscaler1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscaler>> PatchNamespacedHorizontalPodAutoscaler2WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscaler>> PatchNamespacedHorizontalPodAutoscalerStatus1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscaler>> PatchNamespacedHorizontalPodAutoscalerStatus2WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscaler>> PatchNamespacedHorizontalPodAutoscalerStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscaler>> PatchNamespacedHorizontalPodAutoscalerWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1Ingress>> PatchNamespacedIngress1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1Ingress>> PatchNamespacedIngressStatus1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1Ingress>> PatchNamespacedIngressStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1Ingress>> PatchNamespacedIngressWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Job>> PatchNamespacedJobStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Job>> PatchNamespacedJobWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Lease>> PatchNamespacedLease1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Lease>> PatchNamespacedLeaseWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LimitRange>> PatchNamespacedLimitRangeWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NetworkPolicy>> PatchNamespacedNetworkPolicyWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> PatchNamespacedPersistentVolumeClaimStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> PatchNamespacedPersistentVolumeClaimWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudget>> PatchNamespacedPodDisruptionBudgetStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudget>> PatchNamespacedPodDisruptionBudgetWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PodPreset>> PatchNamespacedPodPresetWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> PatchNamespacedPodStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodTemplate>> PatchNamespacedPodTemplateWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> PatchNamespacedPodWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> PatchNamespacedReplicaSetScaleWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSet>> PatchNamespacedReplicaSetStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSet>> PatchNamespacedReplicaSetWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> PatchNamespacedReplicationControllerScaleWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationController>> PatchNamespacedReplicationControllerStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationController>> PatchNamespacedReplicationControllerWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> PatchNamespacedResourceQuotaStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> PatchNamespacedResourceQuotaWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1Role>> PatchNamespacedRole1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Role>> PatchNamespacedRole2WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleBinding>> PatchNamespacedRoleBinding1WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleBinding>> PatchNamespacedRoleBinding2WithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleBinding>> PatchNamespacedRoleBindingWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Role>> PatchNamespacedRoleWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Secret>> PatchNamespacedSecretWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceAccount>> PatchNamespacedServiceAccountWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Service>> PatchNamespacedServiceStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Service>> PatchNamespacedServiceWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> PatchNamespacedStatefulSetScaleWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSet>> PatchNamespacedStatefulSetStatusWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSet>> PatchNamespacedStatefulSetWithHttpMessagesAsync(V1Patch body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> PatchNamespaceStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> PatchNamespaceWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Node>> PatchNodeStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Node>> PatchNodeWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> PatchPersistentVolumeStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> PatchPersistentVolumeWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodSecurityPolicy>> PatchPodSecurityPolicyWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityClass>> PatchPriorityClass1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PriorityClass>> PatchPriorityClass2WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PriorityClass>> PatchPriorityClassWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfiguration>> PatchPriorityLevelConfigurationStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfiguration>> PatchPriorityLevelConfigurationWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RuntimeClass>> PatchRuntimeClass1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RuntimeClass>> PatchRuntimeClassWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1StorageClass>> PatchStorageClass1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StorageClass>> PatchStorageClassWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ValidatingWebhookConfiguration>> PatchValidatingWebhookConfiguration1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ValidatingWebhookConfiguration>> PatchValidatingWebhookConfigurationWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1VolumeAttachment>> PatchVolumeAttachment1WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1VolumeAttachment>> PatchVolumeAttachment2WithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> PatchVolumeAttachmentStatusWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> PatchVolumeAttachmentWithHttpMessagesAsync(V1Patch body, string name, string dryRun = null, string fieldManager = null, bool? force = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIService>> ReadAPIService1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIService>> ReadAPIServiceStatus1WithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIService>> ReadAPIServiceStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIService>> ReadAPIServiceWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1AuditSink>> ReadAuditSinkWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> ReadCertificateSigningRequestStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> ReadCertificateSigningRequestWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRole>> ReadClusterRole1WithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRole>> ReadClusterRole2WithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRoleBinding>> ReadClusterRoleBinding1WithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRoleBinding>> ReadClusterRoleBinding2WithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRoleBinding>> ReadClusterRoleBindingWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRole>> ReadClusterRoleWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ComponentStatus>> ReadComponentStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSIDriver>> ReadCSIDriver1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSIDriver>> ReadCSIDriverWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSINode>> ReadCSINode1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSINode>> ReadCSINodeWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinition>> ReadCustomResourceDefinition1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinition>> ReadCustomResourceDefinitionStatus1WithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinition>> ReadCustomResourceDefinitionStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinition>> ReadCustomResourceDefinitionWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchema>> ReadFlowSchemaStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchema>> ReadFlowSchemaWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1IngressClass>> ReadIngressClassWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1MutatingWebhookConfiguration>> ReadMutatingWebhookConfiguration1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1MutatingWebhookConfiguration>> ReadMutatingWebhookConfigurationWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ConfigMap>> ReadNamespacedConfigMapWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ControllerRevision>> ReadNamespacedControllerRevisionWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJob>> ReadNamespacedCronJob1WithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJob>> ReadNamespacedCronJobStatus1WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJob>> ReadNamespacedCronJobStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJob>> ReadNamespacedCronJobWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSet>> ReadNamespacedDaemonSetStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSet>> ReadNamespacedDaemonSetWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReadNamespacedDeploymentScaleWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Deployment>> ReadNamespacedDeploymentStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Deployment>> ReadNamespacedDeploymentWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EndpointSlice>> ReadNamespacedEndpointSliceWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Endpoints>> ReadNamespacedEndpointsWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Event>> ReadNamespacedEvent1WithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Event>> ReadNamespacedEventWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscaler>> ReadNamespacedHorizontalPodAutoscaler1WithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscaler>> ReadNamespacedHorizontalPodAutoscaler2WithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscaler>> ReadNamespacedHorizontalPodAutoscalerStatus1WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscaler>> ReadNamespacedHorizontalPodAutoscalerStatus2WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscaler>> ReadNamespacedHorizontalPodAutoscalerStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscaler>> ReadNamespacedHorizontalPodAutoscalerWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1Ingress>> ReadNamespacedIngress1WithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1Ingress>> ReadNamespacedIngressStatus1WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1Ingress>> ReadNamespacedIngressStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1Ingress>> ReadNamespacedIngressWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Job>> ReadNamespacedJobStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Job>> ReadNamespacedJobWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Lease>> ReadNamespacedLease1WithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Lease>> ReadNamespacedLeaseWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LimitRange>> ReadNamespacedLimitRangeWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NetworkPolicy>> ReadNamespacedNetworkPolicyWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> ReadNamespacedPersistentVolumeClaimStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> ReadNamespacedPersistentVolumeClaimWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudget>> ReadNamespacedPodDisruptionBudgetStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudget>> ReadNamespacedPodDisruptionBudgetWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Stream>> ReadNamespacedPodLogWithHttpMessagesAsync(string name, string namespaceParameter, string container = null, bool? follow = null, bool? insecureSkipTLSVerifyBackend = null, int? limitBytes = null, string pretty = null, bool? previous = null, int? sinceSeconds = null, int? tailLines = null, bool? timestamps = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PodPreset>> ReadNamespacedPodPresetWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> ReadNamespacedPodStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodTemplate>> ReadNamespacedPodTemplateWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> ReadNamespacedPodWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReadNamespacedReplicaSetScaleWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSet>> ReadNamespacedReplicaSetStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSet>> ReadNamespacedReplicaSetWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReadNamespacedReplicationControllerScaleWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationController>> ReadNamespacedReplicationControllerStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationController>> ReadNamespacedReplicationControllerWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> ReadNamespacedResourceQuotaStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> ReadNamespacedResourceQuotaWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1Role>> ReadNamespacedRole1WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Role>> ReadNamespacedRole2WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleBinding>> ReadNamespacedRoleBinding1WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleBinding>> ReadNamespacedRoleBinding2WithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleBinding>> ReadNamespacedRoleBindingWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Role>> ReadNamespacedRoleWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Secret>> ReadNamespacedSecretWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceAccount>> ReadNamespacedServiceAccountWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Service>> ReadNamespacedServiceStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Service>> ReadNamespacedServiceWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReadNamespacedStatefulSetScaleWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSet>> ReadNamespacedStatefulSetStatusWithHttpMessagesAsync(string name, string namespaceParameter, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSet>> ReadNamespacedStatefulSetWithHttpMessagesAsync(string name, string namespaceParameter, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> ReadNamespaceStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> ReadNamespaceWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Node>> ReadNodeStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Node>> ReadNodeWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> ReadPersistentVolumeStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> ReadPersistentVolumeWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodSecurityPolicy>> ReadPodSecurityPolicyWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityClass>> ReadPriorityClass1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PriorityClass>> ReadPriorityClass2WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PriorityClass>> ReadPriorityClassWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfiguration>> ReadPriorityLevelConfigurationStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfiguration>> ReadPriorityLevelConfigurationWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RuntimeClass>> ReadRuntimeClass1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RuntimeClass>> ReadRuntimeClassWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1StorageClass>> ReadStorageClass1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StorageClass>> ReadStorageClassWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ValidatingWebhookConfiguration>> ReadValidatingWebhookConfiguration1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ValidatingWebhookConfiguration>> ReadValidatingWebhookConfigurationWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1VolumeAttachment>> ReadVolumeAttachment1WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1VolumeAttachment>> ReadVolumeAttachment2WithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> ReadVolumeAttachmentStatusWithHttpMessagesAsync(string name, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> ReadVolumeAttachmentWithHttpMessagesAsync(string name, bool? exact = null, bool? export = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIService>> ReplaceAPIService1WithHttpMessagesAsync(V1beta1APIService body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1APIService>> ReplaceAPIServiceStatus1WithHttpMessagesAsync(V1beta1APIService body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIService>> ReplaceAPIServiceStatusWithHttpMessagesAsync(V1APIService body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1APIService>> ReplaceAPIServiceWithHttpMessagesAsync(V1APIService body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1AuditSink>> ReplaceAuditSinkWithHttpMessagesAsync(V1alpha1AuditSink body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> ReplaceCertificateSigningRequestApprovalWithHttpMessagesAsync(V1beta1CertificateSigningRequest body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> ReplaceCertificateSigningRequestStatusWithHttpMessagesAsync(V1beta1CertificateSigningRequest body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CertificateSigningRequest>> ReplaceCertificateSigningRequestWithHttpMessagesAsync(V1beta1CertificateSigningRequest body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ReplaceClusterCustomObjectScaleWithHttpMessagesAsync(object body, string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ReplaceClusterCustomObjectStatusWithHttpMessagesAsync(object body, string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ReplaceClusterCustomObjectWithHttpMessagesAsync(object body, string group, string version, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRole>> ReplaceClusterRole1WithHttpMessagesAsync(V1alpha1ClusterRole body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRole>> ReplaceClusterRole2WithHttpMessagesAsync(V1beta1ClusterRole body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1ClusterRoleBinding>> ReplaceClusterRoleBinding1WithHttpMessagesAsync(V1alpha1ClusterRoleBinding body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ClusterRoleBinding>> ReplaceClusterRoleBinding2WithHttpMessagesAsync(V1beta1ClusterRoleBinding body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRoleBinding>> ReplaceClusterRoleBindingWithHttpMessagesAsync(V1ClusterRoleBinding body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ClusterRole>> ReplaceClusterRoleWithHttpMessagesAsync(V1ClusterRole body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSIDriver>> ReplaceCSIDriver1WithHttpMessagesAsync(V1beta1CSIDriver body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSIDriver>> ReplaceCSIDriverWithHttpMessagesAsync(V1CSIDriver body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CSINode>> ReplaceCSINode1WithHttpMessagesAsync(V1beta1CSINode body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CSINode>> ReplaceCSINodeWithHttpMessagesAsync(V1CSINode body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinition>> ReplaceCustomResourceDefinition1WithHttpMessagesAsync(V1beta1CustomResourceDefinition body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CustomResourceDefinition>> ReplaceCustomResourceDefinitionStatus1WithHttpMessagesAsync(V1beta1CustomResourceDefinition body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinition>> ReplaceCustomResourceDefinitionStatusWithHttpMessagesAsync(V1CustomResourceDefinition body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1CustomResourceDefinition>> ReplaceCustomResourceDefinitionWithHttpMessagesAsync(V1CustomResourceDefinition body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchema>> ReplaceFlowSchemaStatusWithHttpMessagesAsync(V1alpha1FlowSchema body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1FlowSchema>> ReplaceFlowSchemaWithHttpMessagesAsync(V1alpha1FlowSchema body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1IngressClass>> ReplaceIngressClassWithHttpMessagesAsync(V1beta1IngressClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1MutatingWebhookConfiguration>> ReplaceMutatingWebhookConfiguration1WithHttpMessagesAsync(V1beta1MutatingWebhookConfiguration body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1MutatingWebhookConfiguration>> ReplaceMutatingWebhookConfigurationWithHttpMessagesAsync(V1MutatingWebhookConfiguration body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ConfigMap>> ReplaceNamespacedConfigMapWithHttpMessagesAsync(V1ConfigMap body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ControllerRevision>> ReplaceNamespacedControllerRevisionWithHttpMessagesAsync(V1ControllerRevision body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJob>> ReplaceNamespacedCronJob1WithHttpMessagesAsync(V2alpha1CronJob body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2alpha1CronJob>> ReplaceNamespacedCronJobStatus1WithHttpMessagesAsync(V2alpha1CronJob body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJob>> ReplaceNamespacedCronJobStatusWithHttpMessagesAsync(V1beta1CronJob body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1CronJob>> ReplaceNamespacedCronJobWithHttpMessagesAsync(V1beta1CronJob body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ReplaceNamespacedCustomObjectScaleWithHttpMessagesAsync(object body, string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ReplaceNamespacedCustomObjectStatusWithHttpMessagesAsync(object body, string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> ReplaceNamespacedCustomObjectWithHttpMessagesAsync(object body, string group, string version, string namespaceParameter, string plural, string name, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSet>> ReplaceNamespacedDaemonSetStatusWithHttpMessagesAsync(V1DaemonSet body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1DaemonSet>> ReplaceNamespacedDaemonSetWithHttpMessagesAsync(V1DaemonSet body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReplaceNamespacedDeploymentScaleWithHttpMessagesAsync(V1Scale body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Deployment>> ReplaceNamespacedDeploymentStatusWithHttpMessagesAsync(V1Deployment body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Deployment>> ReplaceNamespacedDeploymentWithHttpMessagesAsync(V1Deployment body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1EndpointSlice>> ReplaceNamespacedEndpointSliceWithHttpMessagesAsync(V1beta1EndpointSlice body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Endpoints>> ReplaceNamespacedEndpointsWithHttpMessagesAsync(V1Endpoints body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Event>> ReplaceNamespacedEvent1WithHttpMessagesAsync(V1beta1Event body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Event>> ReplaceNamespacedEventWithHttpMessagesAsync(V1Event body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscaler>> ReplaceNamespacedHorizontalPodAutoscaler1WithHttpMessagesAsync(V2beta1HorizontalPodAutoscaler body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscaler>> ReplaceNamespacedHorizontalPodAutoscaler2WithHttpMessagesAsync(V2beta2HorizontalPodAutoscaler body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta1HorizontalPodAutoscaler>> ReplaceNamespacedHorizontalPodAutoscalerStatus1WithHttpMessagesAsync(V2beta1HorizontalPodAutoscaler body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V2beta2HorizontalPodAutoscaler>> ReplaceNamespacedHorizontalPodAutoscalerStatus2WithHttpMessagesAsync(V2beta2HorizontalPodAutoscaler body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscaler>> ReplaceNamespacedHorizontalPodAutoscalerStatusWithHttpMessagesAsync(V1HorizontalPodAutoscaler body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1HorizontalPodAutoscaler>> ReplaceNamespacedHorizontalPodAutoscalerWithHttpMessagesAsync(V1HorizontalPodAutoscaler body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1Ingress>> ReplaceNamespacedIngress1WithHttpMessagesAsync(Networkingv1beta1Ingress body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Networkingv1beta1Ingress>> ReplaceNamespacedIngressStatus1WithHttpMessagesAsync(Networkingv1beta1Ingress body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1Ingress>> ReplaceNamespacedIngressStatusWithHttpMessagesAsync(Extensionsv1beta1Ingress body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Extensionsv1beta1Ingress>> ReplaceNamespacedIngressWithHttpMessagesAsync(Extensionsv1beta1Ingress body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Job>> ReplaceNamespacedJobStatusWithHttpMessagesAsync(V1Job body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Job>> ReplaceNamespacedJobWithHttpMessagesAsync(V1Job body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Lease>> ReplaceNamespacedLease1WithHttpMessagesAsync(V1beta1Lease body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Lease>> ReplaceNamespacedLeaseWithHttpMessagesAsync(V1Lease body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1LimitRange>> ReplaceNamespacedLimitRangeWithHttpMessagesAsync(V1LimitRange body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1NetworkPolicy>> ReplaceNamespacedNetworkPolicyWithHttpMessagesAsync(V1NetworkPolicy body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> ReplaceNamespacedPersistentVolumeClaimStatusWithHttpMessagesAsync(V1PersistentVolumeClaim body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolumeClaim>> ReplaceNamespacedPersistentVolumeClaimWithHttpMessagesAsync(V1PersistentVolumeClaim body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudget>> ReplaceNamespacedPodDisruptionBudgetStatusWithHttpMessagesAsync(V1beta1PodDisruptionBudget body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodDisruptionBudget>> ReplaceNamespacedPodDisruptionBudgetWithHttpMessagesAsync(V1beta1PodDisruptionBudget body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PodPreset>> ReplaceNamespacedPodPresetWithHttpMessagesAsync(V1alpha1PodPreset body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> ReplaceNamespacedPodStatusWithHttpMessagesAsync(V1Pod body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PodTemplate>> ReplaceNamespacedPodTemplateWithHttpMessagesAsync(V1PodTemplate body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Pod>> ReplaceNamespacedPodWithHttpMessagesAsync(V1Pod body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReplaceNamespacedReplicaSetScaleWithHttpMessagesAsync(V1Scale body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSet>> ReplaceNamespacedReplicaSetStatusWithHttpMessagesAsync(V1ReplicaSet body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicaSet>> ReplaceNamespacedReplicaSetWithHttpMessagesAsync(V1ReplicaSet body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReplaceNamespacedReplicationControllerScaleWithHttpMessagesAsync(V1Scale body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationController>> ReplaceNamespacedReplicationControllerStatusWithHttpMessagesAsync(V1ReplicationController body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ReplicationController>> ReplaceNamespacedReplicationControllerWithHttpMessagesAsync(V1ReplicationController body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> ReplaceNamespacedResourceQuotaStatusWithHttpMessagesAsync(V1ResourceQuota body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ResourceQuota>> ReplaceNamespacedResourceQuotaWithHttpMessagesAsync(V1ResourceQuota body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1Role>> ReplaceNamespacedRole1WithHttpMessagesAsync(V1alpha1Role body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1Role>> ReplaceNamespacedRole2WithHttpMessagesAsync(V1beta1Role body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RoleBinding>> ReplaceNamespacedRoleBinding1WithHttpMessagesAsync(V1alpha1RoleBinding body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RoleBinding>> ReplaceNamespacedRoleBinding2WithHttpMessagesAsync(V1beta1RoleBinding body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1RoleBinding>> ReplaceNamespacedRoleBindingWithHttpMessagesAsync(V1RoleBinding body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Role>> ReplaceNamespacedRoleWithHttpMessagesAsync(V1Role body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Secret>> ReplaceNamespacedSecretWithHttpMessagesAsync(V1Secret body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ServiceAccount>> ReplaceNamespacedServiceAccountWithHttpMessagesAsync(V1ServiceAccount body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Service>> ReplaceNamespacedServiceStatusWithHttpMessagesAsync(V1Service body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Service>> ReplaceNamespacedServiceWithHttpMessagesAsync(V1Service body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Scale>> ReplaceNamespacedStatefulSetScaleWithHttpMessagesAsync(V1Scale body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSet>> ReplaceNamespacedStatefulSetStatusWithHttpMessagesAsync(V1StatefulSet body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StatefulSet>> ReplaceNamespacedStatefulSetWithHttpMessagesAsync(V1StatefulSet body, string name, string namespaceParameter, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> ReplaceNamespaceFinalizeWithHttpMessagesAsync(V1Namespace body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> ReplaceNamespaceStatusWithHttpMessagesAsync(V1Namespace body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Namespace>> ReplaceNamespaceWithHttpMessagesAsync(V1Namespace body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Node>> ReplaceNodeStatusWithHttpMessagesAsync(V1Node body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1Node>> ReplaceNodeWithHttpMessagesAsync(V1Node body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> ReplacePersistentVolumeStatusWithHttpMessagesAsync(V1PersistentVolume body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PersistentVolume>> ReplacePersistentVolumeWithHttpMessagesAsync(V1PersistentVolume body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PodSecurityPolicy>> ReplacePodSecurityPolicyWithHttpMessagesAsync(V1beta1PodSecurityPolicy body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityClass>> ReplacePriorityClass1WithHttpMessagesAsync(V1alpha1PriorityClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1PriorityClass>> ReplacePriorityClass2WithHttpMessagesAsync(V1beta1PriorityClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1PriorityClass>> ReplacePriorityClassWithHttpMessagesAsync(V1PriorityClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfiguration>> ReplacePriorityLevelConfigurationStatusWithHttpMessagesAsync(V1alpha1PriorityLevelConfiguration body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1PriorityLevelConfiguration>> ReplacePriorityLevelConfigurationWithHttpMessagesAsync(V1alpha1PriorityLevelConfiguration body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1RuntimeClass>> ReplaceRuntimeClass1WithHttpMessagesAsync(V1beta1RuntimeClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1RuntimeClass>> ReplaceRuntimeClassWithHttpMessagesAsync(V1alpha1RuntimeClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1StorageClass>> ReplaceStorageClass1WithHttpMessagesAsync(V1beta1StorageClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1StorageClass>> ReplaceStorageClassWithHttpMessagesAsync(V1StorageClass body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1ValidatingWebhookConfiguration>> ReplaceValidatingWebhookConfiguration1WithHttpMessagesAsync(V1beta1ValidatingWebhookConfiguration body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1ValidatingWebhookConfiguration>> ReplaceValidatingWebhookConfigurationWithHttpMessagesAsync(V1ValidatingWebhookConfiguration body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1alpha1VolumeAttachment>> ReplaceVolumeAttachment1WithHttpMessagesAsync(V1alpha1VolumeAttachment body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1beta1VolumeAttachment>> ReplaceVolumeAttachment2WithHttpMessagesAsync(V1beta1VolumeAttachment body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> ReplaceVolumeAttachmentStatusWithHttpMessagesAsync(V1VolumeAttachment body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<V1VolumeAttachment>> ReplaceVolumeAttachmentWithHttpMessagesAsync(V1VolumeAttachment body, string name, string dryRun = null, string fieldManager = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1APIService>> WatchAPIServiceAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1APIService> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1APIService>> WatchAPIServiceAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1APIService> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1AuditSink>> WatchAuditSinkAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1AuditSink> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1CertificateSigningRequest>> WatchCertificateSigningRequestAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1CertificateSigningRequest> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ClusterRole>> WatchClusterRoleAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ClusterRole> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1ClusterRole>> WatchClusterRoleAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1ClusterRole> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1ClusterRole>> WatchClusterRoleAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1ClusterRole> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ClusterRoleBinding>> WatchClusterRoleBindingAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ClusterRoleBinding> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1ClusterRoleBinding>> WatchClusterRoleBindingAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1ClusterRoleBinding> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1ClusterRoleBinding>> WatchClusterRoleBindingAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1ClusterRoleBinding> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1CSIDriver>> WatchCSIDriverAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1CSIDriver> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1CSIDriver>> WatchCSIDriverAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1CSIDriver> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1CSINode>> WatchCSINodeAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1CSINode> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1CSINode>> WatchCSINodeAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1CSINode> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1CustomResourceDefinition>> WatchCustomResourceDefinitionAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1CustomResourceDefinition> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1CustomResourceDefinition>> WatchCustomResourceDefinitionAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1CustomResourceDefinition> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1FlowSchema>> WatchFlowSchemaAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1FlowSchema> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1IngressClass>> WatchIngressClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1IngressClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1MutatingWebhookConfiguration>> WatchMutatingWebhookConfigurationAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1MutatingWebhookConfiguration> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1MutatingWebhookConfiguration>> WatchMutatingWebhookConfigurationAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1MutatingWebhookConfiguration> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Namespace>> WatchNamespaceAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Namespace> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ConfigMap>> WatchNamespacedConfigMapAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ConfigMap> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ControllerRevision>> WatchNamespacedControllerRevisionAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ControllerRevision> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1CronJob>> WatchNamespacedCronJobAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1CronJob> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V2alpha1CronJob>> WatchNamespacedCronJobAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V2alpha1CronJob> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1DaemonSet>> WatchNamespacedDaemonSetAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1DaemonSet> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Deployment>> WatchNamespacedDeploymentAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Deployment> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Endpoints>> WatchNamespacedEndpointsAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Endpoints> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1EndpointSlice>> WatchNamespacedEndpointSliceAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1EndpointSlice> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Event>> WatchNamespacedEventAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Event> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1Event>> WatchNamespacedEventAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1Event> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1HorizontalPodAutoscaler>> WatchNamespacedHorizontalPodAutoscalerAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1HorizontalPodAutoscaler> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V2beta1HorizontalPodAutoscaler>> WatchNamespacedHorizontalPodAutoscalerAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V2beta1HorizontalPodAutoscaler> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V2beta2HorizontalPodAutoscaler>> WatchNamespacedHorizontalPodAutoscalerAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V2beta2HorizontalPodAutoscaler> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<Extensionsv1beta1Ingress>> WatchNamespacedIngressAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, Extensionsv1beta1Ingress> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<Networkingv1beta1Ingress>> WatchNamespacedIngressAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, Networkingv1beta1Ingress> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Job>> WatchNamespacedJobAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Job> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Lease>> WatchNamespacedLeaseAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Lease> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1Lease>> WatchNamespacedLeaseAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1Lease> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1LimitRange>> WatchNamespacedLimitRangeAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1LimitRange> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1NetworkPolicy>> WatchNamespacedNetworkPolicyAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1NetworkPolicy> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1PersistentVolumeClaim>> WatchNamespacedPersistentVolumeClaimAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1PersistentVolumeClaim> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Pod>> WatchNamespacedPodAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Pod> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1PodDisruptionBudget>> WatchNamespacedPodDisruptionBudgetAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1PodDisruptionBudget> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1PodPreset>> WatchNamespacedPodPresetAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1PodPreset> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1PodTemplate>> WatchNamespacedPodTemplateAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1PodTemplate> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ReplicaSet>> WatchNamespacedReplicaSetAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ReplicaSet> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ReplicationController>> WatchNamespacedReplicationControllerAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ReplicationController> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ResourceQuota>> WatchNamespacedResourceQuotaAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ResourceQuota> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Role>> WatchNamespacedRoleAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Role> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1Role>> WatchNamespacedRoleAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1Role> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1Role>> WatchNamespacedRoleAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1Role> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1RoleBinding>> WatchNamespacedRoleBindingAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1RoleBinding> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1RoleBinding>> WatchNamespacedRoleBindingAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1RoleBinding> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1RoleBinding>> WatchNamespacedRoleBindingAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1RoleBinding> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Secret>> WatchNamespacedSecretAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Secret> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ServiceAccount>> WatchNamespacedServiceAccountAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ServiceAccount> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Service>> WatchNamespacedServiceAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Service> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1StatefulSet>> WatchNamespacedStatefulSetAsync(string name, string @namespace, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1StatefulSet> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1Node>> WatchNodeAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1Node> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<T>> WatchObjectAsync<T>(string path, string @continue = null, string fieldSelector = null, bool? includeUninitialized = null, string labelSelector = null, int? limit = null, bool? pretty = null, int? timeoutSeconds = null, string resourceVersion = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, T> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1PersistentVolume>> WatchPersistentVolumeAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1PersistentVolume> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1PodSecurityPolicy>> WatchPodSecurityPolicyAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1PodSecurityPolicy> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1PriorityClass>> WatchPriorityClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1PriorityClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1PriorityClass>> WatchPriorityClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1PriorityClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1PriorityClass>> WatchPriorityClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1PriorityClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1PriorityLevelConfiguration>> WatchPriorityLevelConfigurationAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1PriorityLevelConfiguration> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1RuntimeClass>> WatchRuntimeClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1RuntimeClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1RuntimeClass>> WatchRuntimeClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1RuntimeClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1StorageClass>> WatchStorageClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1StorageClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1StorageClass>> WatchStorageClassAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1StorageClass> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1ValidatingWebhookConfiguration>> WatchValidatingWebhookConfigurationAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1ValidatingWebhookConfiguration> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1ValidatingWebhookConfiguration>> WatchValidatingWebhookConfigurationAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1ValidatingWebhookConfiguration> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1VolumeAttachment>> WatchVolumeAttachmentAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1VolumeAttachment> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1alpha1VolumeAttachment>> WatchVolumeAttachmentAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1alpha1VolumeAttachment> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Watcher<V1beta1VolumeAttachment>> WatchVolumeAttachmentAsync(string name, bool? allowWatchBookmarks = null, string @continue = null, string fieldSelector = null, string labelSelector = null, int? limit = null, bool? pretty = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, Dictionary<string, List<string>> customHeaders = null, Action<WatchEventType, V1beta1VolumeAttachment> onEvent = null, Action<Exception> onError = null, Action onClosed = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebSocket> WebSocketNamespacedPodAttachAsync(string name, string @namespace, string container = null, bool stderr = true, bool stdin = false, bool stdout = true, bool tty = false, string webSocketSubProtol = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebSocket> WebSocketNamespacedPodExecAsync(string name, string @namespace = "default", string command = null, string container = null, bool stderr = true, bool stdin = true, bool stdout = true, bool tty = true, string webSocketSubProtol = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebSocket> WebSocketNamespacedPodExecAsync(string name, string @namespace = "default", IEnumerable<string> command = null, string container = null, bool stderr = true, bool stdin = true, bool stdout = true, bool tty = true, string webSocketSubProtol = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<WebSocket> WebSocketNamespacedPodPortForwardAsync(string name, string @namespace, IEnumerable<int> ports, string webSocketSubProtocol = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            var uri = new Uri($"wss://127.0.0.1:32772/api/v1/namespaces/{@namespace}/pods/{name}/portforward?ports={ports.First()}");

            return await StreamConnectAsync(uri, webSocketSubProtocol: webSocketSubProtocol);



            //var ws = new ClientWebSocket();
            //var sslCaCerts = new X509Certificate2(Convert.FromBase64String("LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSUM4akNDQWRxZ0F3SUJBZ0lJUHVYQlZGWFZMa3N3RFFZSktvWklodmNOQVFFTEJRQXdGVEVUTUJFR0ExVUUKQXhNS2EzVmlaWEp1WlhSbGN6QWVGdzB5TURBMU1qWXdPREExTURkYUZ3MHlNVEExTWpZd09EQTFNRGxhTURReApGekFWQmdOVkJBb1REbk41YzNSbGJUcHRZWE4wWlhKek1Sa3dGd1lEVlFRREV4QnJkV0psY201bGRHVnpMV0ZrCmJXbHVNSUlCSWpBTkJna3Foa2lHOXcwQkFRRUZBQU9DQVE4QU1JSUJDZ0tDQVFFQW04OHYzVVZTQUdTVDUzdWUKOElXNFk4SHZya3BGemVGRTFWTENMeXpyNnZMVG5LRndaRXMxNGJPMmkybkpLaVk2b2g4em96UWlIcnZDT3U4eQp6VEg3ODRvT1d6UEh1Zy9WSXhhS3ZBZFVPd0dKQ2g1RFV3SmwxL24rd0hwRTRTQjFDakJVeDYzVjhnUW45WjFlCkRFNHNTYXQzeXYrL3R0cWtQbEhWWUlZWndUVjNvTHQ2SjM2Z0pyWW5wb0RuNHdMMUhyTjcwcUwxbmRKQnNyNkYKRzd0ZndpQ1dIM3gxMmhPTFd2MkFlbDdxeXlzT1FMenJJcW55d0RwcHFzK0FrZjRyc0dmSC82SGxaS1NCb3ozdgpkbTlaT2FFNDlIMklyWU1FN0kxZ3lkdXJNVTl3UUN5ZmNmVE43RWZEWXYrdWprMU5MY3NMS0VtditMUWhjekFKCndweFYxd0lEQVFBQm95Y3dKVEFPQmdOVkhROEJBZjhFQkFNQ0JhQXdFd1lEVlIwbEJBd3dDZ1lJS3dZQkJRVUgKQXdJd0RRWUpLb1pJaHZjTkFRRUxCUUFEZ2dFQkFFaUZtd2FWUk00NjIzNkNpZDZKSTltUUFmUWI1WUpYK3FrTAozL1p1NTQ2VFJvTU96dXZhVXlrYU0wN2NOQ0ludGdLRlMxY3oyZW15aEVlNE8zbHoyeE5NR1h3NXBubnl5Snh2CnhjUDZGY1QvYmJvSTVJR2NwNU9VVEloY3ZFemtsckc2V0RpemplOFJpNDFmcFpxbS9JQ1JtdDJlZHZsS2xINE8KSCtvY3hhYnhmOXRxTHJVOU5xL2dXOHVCNGpxOHZrTTVQaGlTSWNMMWFjakV1aGVsNExicnFBQUNXdDlnRnFQRQphR1RjeExtWmhIbjdtcVFYNGxEOEtJTzBmTWN0VzdWQkQwRElWUW9EVmJIQ1BxZmhIbVFxams4cVQ2WnhKVlpVClQ1c2N6VG1FMWdFSmdxZDlzUTJFZHVMVWh6MFFjd0xibDRiSzliUUx6S2tNN1pwOXVkMD0KLS0tLS1FTkQgQ0VSVElGSUNBVEUtLS0tLQo=")); 
            //ws.Options.ClientCertificates.Add(sslCaCerts);
            //ws.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            //ws.Options.AddSubProtocol(webSocketSubProtocol);

            //await ws.ConnectAsync(uri, CancellationToken.None)
            //    .ConfigureAwait(false);
            //return ws;
        }

        protected async Task<WebSocket> StreamConnectAsync(Uri uri, string invocationId = null,
            string webSocketSubProtocol = null, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var config =
                KubernetesClientConfiguration.BuildConfigFromConfigFile(currentContext: "kind-argo-demo-ci");

            var webSocket = new ClientWebSocket();
            var cert = CertUtils.GeneratePfx(config);
            webSocket.Options.ClientCertificates.Add(cert);
            webSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            webSocket.Options.AddSubProtocol(webSocketSubProtocol);

            await webSocket.ConnectAsync(uri, CancellationToken.None)
                .ConfigureAwait(false);
            return webSocket;
        }

    }



}
