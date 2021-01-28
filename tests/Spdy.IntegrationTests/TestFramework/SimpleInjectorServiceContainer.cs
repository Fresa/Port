using System;
using SimpleInjector;
using Test.It;
using Spdy.Helpers;

namespace Spdy.IntegrationTests.TestFramework
{
    internal sealed class SimpleInjectorServiceContainer : IServiceContainer
    {
        private readonly Container _container;

        public SimpleInjectorServiceContainer(
            Container container)
        {
            _container = container;
        }

        private IDisposable OverrideRegistration()
        {
            _container.Options.AllowOverridingRegistrations = true;
            return new DisposableAction(
                () => _container.Options.AllowOverridingRegistrations = false);
        }

        public void Register<TImplementation>(
            Func<TImplementation> configurer) where TImplementation : class
        {
            using (OverrideRegistration())
            {
                _container.Register(configurer);
            }
        }

        public void Register<TService, TImplementation>()
            where TService : class where TImplementation : class, TService
        {
            using (OverrideRegistration())
            {
                _container.Register<TService, TImplementation>();
            }
        }

        public void RegisterSingleton<TImplementation>(
            Func<TImplementation> configurer) where TImplementation : class
        {
            using (OverrideRegistration())
            {
                _container.RegisterSingleton(configurer);
            }
        }

        public void RegisterSingleton<TService, TImplementation>()
            where TService : class where TImplementation : class, TService
        {
            using (OverrideRegistration())
            {
                _container.RegisterSingleton<TService, TImplementation>();
            }
        }

        public TService Resolve<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }

        public void Dispose()
        {
        }
    }
}