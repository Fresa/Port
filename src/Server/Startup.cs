using System.Net.Http;
using Log.It;
using Log.It.With.NLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using SimpleInjector;
using LogFactory = Log.It.LogFactory;

namespace Port.Server
{
    public class Startup
    {
        private readonly Container _container = new Container();

        static Startup()
        {
            LogFactory.Initialize(new NLogFactory(new LogicalThreadContext()));
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
            services.AddControllersWithViews();
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
                    handlers: new DelegatingHandler[]
                    {
                        new LogItHttpMessageHandlerDecorator()
                    }));
            _container.RegisterSingleton<INetworkServerFactory, SocketNetworkServerFactory>();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            app.UseSimpleInjector(_container);

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