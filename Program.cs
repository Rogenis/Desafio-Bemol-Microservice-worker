using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MicroserviceWorker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeuMicroservicoWorker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddSingleton<WorkerService>();

                    var config = hostContext.Configuration;
                    var serviceBusConnectionString = config["ServiceBus:ConnectionString"];
                    var queueName = config["ServiceBus:QueueName"];
                    services.AddSingleton<ServiceBusClient>(new ServiceBusClient(serviceBusConnectionString));
                    services.AddSingleton<ServiceBusProcessor>(new ServiceBusClient(serviceBusConnectionString).CreateProcessor(queueName));
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            var processor = host.Services.GetRequiredService<ServiceBusProcessor>();
            var workerService = host.Services.GetRequiredService<WorkerService>();

            processor.ProcessMessageAsync += async (args) =>
            {
                var message = args.Message;
                var data = message.Body.ToString();

                // Call gRPC service
                var response = await workerService.ProcessItemAsync(new ProcessItemRequest
                {
                    Id = message.MessageId,
                    Data = data
                });

                // Complete the message
                await processor.CompleteMessageAsync(message);
            };


            processor.ProcessErrorAsync += (errorArgs) =>
            {
                Console.WriteLine(errorArgs.Exception.ToString());
                return Task.CompletedTask;
            };

            await host.RunAsync();
        }
    }
}
