using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Port.Client
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(
                _ => new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                });
            return builder.Build()
                          .RunAsync();
        }
    }
}
