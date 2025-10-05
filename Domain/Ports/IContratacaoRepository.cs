using Domain.Entities;

namespace Domain.Ports;

public interface IContratacaoRepository
{
    Task<Contratacao?> GetByPropostaIdAsync(Guid propostaId);
    Task<Contratacao> InsertAsync(Contratacao contratacao);
}
