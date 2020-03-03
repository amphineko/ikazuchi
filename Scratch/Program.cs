using System;
using System.Threading.Tasks;
using Coordinator.Hosting;
using Coordinator.Signaling.Gateway;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;

namespace Scratch
{
    internal class Program
    {
        private static async Task<HubConnection> ConnectToHub()
        {
            var client = new HubConnectionBuilder()
                .WithAutomaticReconnect()
                .WithUrl("http://localhost:5000/api/v1/gateway")
                .Build();
            await client.StartAsync();
            return client;
        }

        private static async Task Main(string[] args)
        {
            var config = new StartupConfiguration
            {
                ClusterId = "dev",
                ServiceId = "Ikazuchi2"
            };

            using var host = Startup.CreateDevelopmentHostBuilder(args, config).Build();
            await host.StartAsync();
            Console.Out.WriteLine("Started localhost hosting");

            // callee setup

            var callee = await ConnectToHub();
            callee.On<Guid, string>("ReceiveRtcOffer", (origin, offer) =>
            {
                Console.Out.WriteLine($"Received offer from {origin}: {offer}");
                callee.InvokeAsync(nameof(IGatewayServer.SendRtcAnswer), origin, "EXAMPLE ANSWER");
            });

            var calleeId = await callee.InvokeAsync<Guid>(nameof(IGatewayServer.GetParticipantId));
            Console.Out.WriteLine($"Callee Id: {calleeId}");

            // initiate session

            var sessionId = Guid.NewGuid();
            await callee.InvokeAsync(nameof(IGatewayServer.JoinSession), sessionId);

            // caller setup

            var caller = await ConnectToHub();
            caller.On<Guid, string>("ReceiveRtcAnswer", (origin, answer) =>
            {
                Console.Out.WriteLine($"Received answer from {origin}: {answer}");
                host.StopAsync();
            });

            var callerId = await caller.InvokeAsync<Guid>(nameof(IGatewayServer.GetParticipantId));
            Console.Out.WriteLine($"Caller Id: {callerId}");

            // join session

            await caller.InvokeAsync(nameof(IGatewayServer.JoinSession), sessionId);

            // send rtc offer

            await caller.InvokeAsync(nameof(IGatewayServer.SendRtcOffer), calleeId, "EXAMPLE OFFER");

            await Task.Delay(TimeSpan.FromSeconds(2.5));

            await host.WaitForShutdownAsync();
        }
    }
}