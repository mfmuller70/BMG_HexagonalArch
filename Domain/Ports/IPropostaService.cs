using Domain.Entities;

namespace Domain.Ports;

public interface IPropostaService
{
    Task<IEnumerable<Proposta>> GetAllPropostasAsync();
    Task<Proposta?> GetPropostaByIdAsync(Guid id);
    Task<Proposta> CriarPropostaAsync(string clienteNome, decimal valorCobertura);
    Task<Proposta> AtualizarStatusPropostaAsync(Guid id, StatusProposta novoStatus);
    Task<IEnumerable<Proposta>> GetPropostasByStatusAsync(StatusProposta status);
}
