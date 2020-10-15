using System.Net.Http;
using Log.It;
using Log.It.With.NLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using NLog;
using NLog.Extensions.Logging;
using Port.Server.Observability;
using SimpleInjector;

namespace Port.Server
{
    public class Startup
    {
        private readonly Container _container = new Container();

        static Startup()
        {
            LogFactoryExtensions.InitializeOnce(new NLogFactory(new LogicalThreadContext()));
        }

        public Startup(
            IConfiguration configuration)
        {
            Configuration = configuration;
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

            services.AddLogging(
                builder =>
                {
                    builder.ClearProviders();
                    builder.AddNLog(Configuration);
                    var nLogConfig = new NLogLoggingConfiguration(
                        Configuration.GetSection("NLog"));
                    LogManager.Configuration = nLogConfig;
                });

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
                    createHandlers: () => new DelegatingHandler[]
                    {
                        new LogItHttpMessageHandlerDecorator()
                    }));
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
                async () => await _container.DisposeAsync()
                    .ConfigureAwait(false));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

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