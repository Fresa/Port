using System.Net.Http;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Port.Server.DependencyInjection;
using Port.Server.Hosting;
using Port.Server.Observability;
using Port.Server.Services;
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

            services.AddSingleton(_container);
            services.AddSingleton(
                typeof(IGrpcServiceActivator<>),
                typeof(GrpcSimpleInjectorActivator<>));

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

            services.AddGrpc(options => options.EnableDetailedErrors = true);

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
                            new LogItHttpMessageHandlerDecorator(handler))));
            _container
                .RegisterSingleton<INetworkServerFactory,
                    SocketNetworkServerFactory>();
            _container.RegisterSingleton<PortForwardService>();
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

            app.UseExceptionHandler(
                builder => builder.Run(env.HandleExceptions));

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseGrpcWeb(
                new GrpcWebOptions
                {
                    DefaultEnabled = true
                });

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapGrpcService<PortForwardService>();
                    endpoints.MapFallbackToFile("index.html");
                });

            _container.Verify();
        }
    }
}