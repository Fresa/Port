using System;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server;
using SimpleInjector;

namespace Port.Server.DependencyInjection
{
    internal sealed class GrpcSimpleInjectorActivator<T> : IGrpcServiceActivator<T>
        where T : class
    {
        private readonly Container _container;

        public GrpcSimpleInjectorActivator(
            Container container) => _container = container;

        public GrpcActivatorHandle<T> Create(
            IServiceProvider serviceProvider) =>
            new(_container.GetInstance<T>(), false, null);

        public ValueTask ReleaseAsync(
            GrpcActivatorHandle<T> service) => default;
    }
}