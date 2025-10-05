namespace Domain.Ports;

public interface IMessageService
{
    Task PublishAsync<T>(string topic, T message);
    Task SubscribeAsync<T>(string topic, Func<T, Task> handler);
}
