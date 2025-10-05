using Domain.Entities;

namespace Domain.Ports;

public interface IContratacaoService
{
    Task<Contratacao> ContratarPropostaAsync(Guid propostaId);
    Task<Proposta?> VerificarStatusPropostaAsync(Guid propostaId);
}
