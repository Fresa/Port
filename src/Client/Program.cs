using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
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

            builder.Services.AddSingleton(
                _ =>
                    GrpcChannel.ForAddress(
                        builder.HostEnvironment.BaseAddress,
                        new GrpcChannelOptions
                        {
                            HttpHandler = new GrpcWebHandler(
                                GrpcWebMode.GrpcWebText,
                                new HttpClientHandler())
                        }));

            return builder.Build()
                          .RunAsync();
        }
    }
}
