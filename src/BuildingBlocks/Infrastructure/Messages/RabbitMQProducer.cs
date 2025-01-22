using System.Text;

using Contracts.Common.Interfaces;
using Contracts.Messages;

using RabbitMQ.Client;

namespace Infrastructure.Messages;

public class RabbitMQProducer : IMessageProducer
{
    private readonly ISerializeService _serializeService;

    public RabbitMQProducer(ISerializeService serializeService)
    {
        _serializeService = serializeService;
    }
    public async void SendMessage<T>(T message)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        IConnection connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queue: "order",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        var jsonData = _serializeService.Serialize(message);

        var body = Encoding.UTF8.GetBytes(jsonData);
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = DeliveryModes.Persistent;

        await channel.BasicPublishAsync(exchange: string.Empty,
            routingKey: "orders",
            mandatory: false,
            basicProperties: props,
            body: body);
    }
}