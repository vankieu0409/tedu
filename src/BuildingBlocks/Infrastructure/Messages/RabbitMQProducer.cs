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
    public void SendMessage<T>(T message)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "order",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var jsonData = _serializeService.Serialize(message);

        var body = Encoding.UTF8.GetBytes(jsonData);

        channel.BasicPublish(exchange: "",
                             routingKey: "orders",
                             basicProperties: null,
                             body: body);


    }
}