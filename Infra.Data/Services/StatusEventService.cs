using Domain.Entities;
using Domain.Ports;
using Microsoft.Extensions.Logging;

namespace Infra.Data.Services;

public class StatusEventService
{
    private readonly IMessageService _messageService;
    private readonly ILogger<StatusEventService> _logger;

    public StatusEventService(IMessageService messageService, ILogger<StatusEventService> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    // Producer - Publica mudança de status (mensagem simples)
    public async Task PublicarMudancaStatusAsync(Guid propostaId, StatusProposta statusAnterior, StatusProposta novoStatus)
    {
        try
        {
            var evento = new
            {
                PropostaId = propostaId,
                StatusAnterior = statusAnterior.ToString(),
                NovoStatus = novoStatus.ToString(),
                Timestamp = DateTime.UtcNow,
                Evento = "MudancaStatus"
            };

            // Publica mensagem simples em uma fila única
            await _messageService.PublishAsync("status", evento);
            _logger.LogInformation("Evento de mudança de status publicado: {PropostaId} - {StatusAnterior} -> {NovoStatus}",
                propostaId, statusAnterior, novoStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento de mudança de status para proposta {PropostaId}", propostaId);
            throw;
        }
    }

    // Consumer - Processa mudança de status (fila única)
    public async Task ConsumirMudancaStatusAsync(string status, Func<object, Task> handler)
    {
        try
        {
            await _messageService.SubscribeAsync<object>("status", handler);
            _logger.LogInformation("Consumer registrado para fila de status");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar consumer para fila de status");
            throw;
        }
    }
}
