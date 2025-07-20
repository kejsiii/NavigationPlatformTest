
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Application.Services.Messaging
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly RabbitMQ.Client.IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;
        private readonly string _exchangeName;
        private readonly ILogger<RabbitMqEventPublisher> _logger;

        public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventPublisher> logger)
        {
            _logger = logger;
            var config = options.Value;
            _exchangeName = config.ExchangeName;

            var factory = new ConnectionFactory
            {
                HostName = config.HostName
            };

            _connection = factory.CreateConnection(); 
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout, durable: true);
        }

        public Task PublishAsync(string eventName, object payload, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: "",
                basicProperties: null,
                body: body
            );

            _logger.LogInformation("Published event: {EventName}", eventName);
            return Task.CompletedTask;
        }
    }
}
