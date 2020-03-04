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
        private const string HubCorsPolicyName = "hub-cors-policy";

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

            app.UseCors(HubCorsPolicyName);
            app.UseAuthorization();

            app.UseEndpoints(builder =>
            {
                builder
                    .MapHub<GatewayController>("/api/v1/gateway")
                    .RequireCors(HubCorsPolicyName);
            });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(HubCorsPolicyName, builder =>
                {
                    builder
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:3000");
                });
            });
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