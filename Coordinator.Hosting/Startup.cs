using System.Net;
using System.Threading.Tasks;
using Coordinator.Signaling.Components;
using Coordinator.Signaling.Gateway;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Coordinator.Hosting
{
    public static class Startup
    {
        public static IHostBuilder CreateDevelopmentHostBuilder(string[] args, StartupConfiguration config)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .ConfigureWebHostDefaults(builder => builder.Configure(ConfigureWebApp))
                .UseOrleans(builder =>
                {
                    builder
                        .ConfigureApplicationParts(manager =>
                        {
                            manager.AddApplicationPart(typeof(SessionParticipantGrain).Assembly).WithReferences();
                        })
                        .AddMemoryGrainStorageAsDefault()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = config.ClusterId;
                            options.ServiceId = config.ServiceId;
                        })
                        .Configure<EndpointOptions>(options => { options.AdvertisedIPAddress = IPAddress.Loopback; })
                        .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
                        .UseLocalhostClustering();
                });
        }

        private static void ConfigureWebApp(WebHostBuilderContext ctx, IApplicationBuilder app)
        {
            if (ctx.HostingEnvironment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(builder => { builder.MapHub<GatewayController>("/api/v1/gateway"); });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddConsole());
            services.AddSignalR();
        }

        private static async Task Main(string[] args)
        {
            var config = new StartupConfiguration
            {
                ClusterId = "dev",
                ServiceId = "Ikazuchi2"
            };

            using var host = CreateDevelopmentHostBuilder(args, config).Build();
            await host.RunAsync();
        }
    }
}