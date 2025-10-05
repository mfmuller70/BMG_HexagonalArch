using Domain.Entities;

namespace Domain.Ports;

public interface IPropostaRepository
{
    Task<IEnumerable<Proposta>> GetAllAsync();
    Task<Proposta?> GetByIdAsync(Guid id);
    Task<Proposta> InsertAsync(Proposta proposta);
    Task<Proposta> UpdateAsync(Proposta proposta);
    Task<IEnumerable<Proposta>> GetByStatusAsync(StatusProposta status);
}
