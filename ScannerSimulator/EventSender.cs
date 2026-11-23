using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ScannerSimulator;

public class EventSender : IDisposable
{
    private readonly IConnection connection;
    private readonly IModel channel;

    public EventSender()
    {
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        channel.QueueDeclare("visit-events", true, false, false, null);
    }

    public void Send(VisitEvent visitEvent)
    {
        var json = JsonSerializer.Serialize(visitEvent);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(
            exchange: "",
            routingKey: "visit-events",
            basicProperties: properties,
            body: body);
    }

    public void Dispose()
    {
        channel.Dispose();
        connection.Dispose();
    }
}