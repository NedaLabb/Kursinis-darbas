using System.Text.Json;
using GymOccupancy.Models;
using RabbitMQ.Client;

namespace GymOccupancy.Services;

public class RabbitMqReceiver : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;

    public RabbitMqReceiver(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare("visit-events", true, false, false, null);

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = channel.BasicGet("visit-events", false);

            if (message == null)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }

            try
            {
                var visitEvent = JsonSerializer.Deserialize<VisitEvent>(
                    message.Body.ToArray());

                if (visitEvent == null)
                {
                    throw new JsonException("Pranešime nėra įvykio.");
                }

                using var scope = scopeFactory.CreateScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<EventProcessor>();

                var result = await processor.ProcessAsync(
                    visitEvent.CompanyId, visitEvent);

                Console.WriteLine(result);

                channel.BasicAck(message.DeliveryTag, false);
            }
            catch (JsonException)
            {
                Console.WriteLine("Atmestas netinkamo formato pranešimas.");
                channel.BasicNack(message.DeliveryTag, false, false);
            }
            catch (Exception error)
            {
                Console.WriteLine($"Apdorojimo klaida: {error.Message}");
                channel.BasicNack(message.DeliveryTag, false, true);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}