using Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Infra.Data.Adapters;

public class RabbitMQService : IMessageService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _virtualHost;
    private readonly string _queueName;

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        _userName = configuration["RabbitMQ:UserName"] ?? "guest";
        _password = configuration["RabbitMQ:Password"] ?? "guest";
        _virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
        _queueName = configuration["RabbitMQ:QueueName"] ?? "status.queue";

        _logger.LogInformation("Configurando RabbitMQ - Host: {HostName}, Port: {Port}, User: {UserName}, VHost: {VirtualHost}",
            _hostName, _port, _userName, _virtualHost);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                Port = _port,
                UserName = _userName,
                Password = _password,
                VirtualHost = _virtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                RequestedHeartbeat = TimeSpan.FromSeconds(60)
            };

            _logger.LogInformation("Criando conexão com RabbitMQ...");
            _connection = factory.CreateConnection();
            _logger.LogInformation("Conexão criada com sucesso!");

            _logger.LogInformation("Criando channel...");
            _channel = _connection.CreateModel();
            _logger.LogInformation("Channel criado com sucesso!");

            // Declarar apenas uma fila simples
            try
            {
                _logger.LogInformation("Criando queue: {QueueName}", _queueName);
                _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
                _logger.LogInformation("Queue {QueueName} criada com sucesso!", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar queue {QueueName}: {Message}", _queueName, ex.Message);
                throw;
            }

            _logger.LogInformation("RabbitMQ conectado com sucesso! Usando fila: {Queue}", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar no RabbitMQ: {Message}", ex.Message);
            throw;
        }
    }

    public async Task PublishAsync<T>(string topic, T message)
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            // Publicar direto na fila usando a default exchange
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _queueName,
                basicProperties: props,
                body: body
            );

            _logger.LogInformation("Mensagem publicada na fila {Queue}: {Message}", _queueName, messageBody);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem na fila {Queue}", _queueName);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string topic, Func<T, Task> handler)
    {
        try
        {
            var queueToUse = _queueName;

            var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var deserializedMessage = JsonSerializer.Deserialize<T>(message);

                    if (deserializedMessage != null)
                    {
                        await handler(deserializedMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem na fila {Queue}", queueToUse);
                }
            };

            _channel.BasicConsume(queue: queueToUse, autoAck: true, consumer: consumer);
            _logger.LogInformation("Inscrito na fila {Queue}", queueToUse);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao se inscrever na fila {Queue}", _queueName);
            throw;
        }
    }


    public void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        try { _channel?.Dispose(); } catch { }
        try { _connection?.Dispose(); } catch { }
    }
}
