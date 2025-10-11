using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace GhostCodeBackend.Shared.RPC.MessageBroker;


public class RabbitMQService : IRabbitMQService, IAsyncDisposable
{
    private IConnection _connection;
    private IChannel _channel;
    private readonly HashSet<string> _declaredQueues = new();
    

    private bool _initialized = false;
    
    public RabbitMQService(IConnection connection)
    {
        _connection = connection;
    }

    public async Task InitializeAsync()
    {
        _channel = await _connection.CreateChannelAsync();
        
        _initialized = true;
    }

    public async Task EnsureQueueExistsAsync(string queueName)
    {
        //_logger.LogInformation("Ensure queue {queueName}", queueName);
        if (!_initialized) return;
        //_logger.LogInformation("Ensure queue {queueName} REAL", queueName);
        
        if (_declaredQueues.Contains(queueName))
            return;

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _declaredQueues.Add(queueName);
    }

    public async Task SendMessageAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
    {
        //_logger.LogInformation("Sending message {message}", JsonSerializer.Serialize(message));
        if (!_initialized) return;
        await EnsureQueueExistsAsync(queueName);
        //_logger.LogInformation("Sending message {message} REAL", JsonSerializer.Serialize(message));
        
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
            
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task StartConsumingAsync<T>(string queueName, Func<T, Task> messageHandler, 
                                           CancellationToken cancellationToken = default)
    {
        //_logger.LogInformation("Starting consuming queue {QueueName}", queueName);
        if (!_initialized) return;
        await EnsureQueueExistsAsync(queueName);
        //_logger.LogInformation("Starting consuming queue {QueueName} REAL", queueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                
                var message = JsonSerializer.Deserialize<T>(messageJson);
                
                if (message != null)
                {
                    await messageHandler(message);
                }

                await _channel.BasicAckAsync(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                
                await _channel.BasicNackAsync(args.DeliveryTag, false, false);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
        
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel?.IsOpen == true)
            await _channel.CloseAsync();
        
        if (_connection?.IsOpen == true)
            await _connection.CloseAsync();

        _channel?.Dispose();
        _connection?.Dispose();
        
    }
}
