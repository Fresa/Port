using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Port.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(
                    args
                        .Append($"--{WebHostDefaults.ContentRootKey}")
                        .Append(GetBasePath())
                        .ToArray())
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(params string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders();
                })
                .ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    configurationBuilder
                        .AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .UseNLog();

        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName) ?? AppContext.BaseDirectory;
        }
    }
}
