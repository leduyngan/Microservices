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
        var connectionFactory = new ConnectionFactory
        {
            HostName = "192.168.1.8",  
            Port = 5672 // Cổng mặc định cho RabbitMQ  
        };
        
        var connection = connectionFactory.CreateConnection();
        using var channel = connection.CreateModel(); 
        channel.QueueDeclare("orders", exclusive: false);
        
        var jsonData = _serializeService.Serialize(message);
        var body = Encoding.UTF8.GetBytes(jsonData);
        
        channel.BasicPublish("", "orders", null, body);
    }
}