using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Port.Server.Hosting;
using Port.Server.Observability;
using SimpleInjector;

namespace Port.Server
{
    public class Startup
    {
        private readonly Container _container = new();

        static Startup()
        {
            LogFactoryExtensions.InitializeOnce();
        }

        public Startup(
            IConfiguration configuration)
        {
            Configuration = configuration;
            NLogBuilderExtensions.ConfigureNLogOnce(configuration);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(
            IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddNewtonsoftJson();
            services.AddRazorPages();

            services.AddSimpleInjector(
                _container, options =>
                {
                    options
                        .AddAspNetCore()
                        .AddControllerActivation()
                        .AddViewComponentActivation()
                        .AddPageModelActivation()
                        .AddTagHelperActivation();

                    options.AddLogging();
                });

            services.AddFeatureManagement();

            InitializeContainer();
        }

        private void InitializeContainer()
        {
            _container
                .RegisterSingleton<IKubernetesService, KubernetesService>();
            _container
                .RegisterSingleton<IKubernetesClientFactory,
                    KubernetesClientFactory>();
            _container.RegisterSingleton(
                () => new KubernetesConfiguration(
                    createClient: handler =>
                        new HttpClient(
                            new LogItHttpMessageHandlerDecorator(
                                handler))
                    ));
            _container
                .RegisterSingleton<INetworkServerFactory,
                    SocketNetworkServerFactory>();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            app.UseSimpleInjector(_container);
            hostApplicationLifetime.ApplicationStopped.Register(
                () => _container.DisposeAsync());

            if (env.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseHsts();
            }

            app.UseExceptionHandler(builder => builder.Run(env.HandleExceptions));

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("index.html");
                });

            _container.Verify();
        }
    }
}