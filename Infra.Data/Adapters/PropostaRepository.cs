using Domain.Entities;
using Domain.Ports;
using Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data.Adapters;

public class PropostaRepository : IPropostaRepository
{
    private readonly SqlServerContext _context;

    public PropostaRepository(SqlServerContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Proposta>> GetAllAsync()
    {
        return await _context.Propostas.ToListAsync();
    }

    public async Task<Proposta?> GetByIdAsync(Guid id)
    {
        return await _context.Propostas.FirstOrDefaultAsync(p => p.PropostaId == id);
    }

    public async Task<Proposta> InsertAsync(Proposta proposta)
    {
        if (proposta is null)
        {
            throw new ArgumentNullException(nameof(proposta));
        }

        _context.Propostas.Add(proposta);
        await _context.SaveChangesAsync();
        return proposta;
    }

    public async Task<Proposta> UpdateAsync(Proposta proposta)
    {
        if (proposta is null)
        {
            throw new ArgumentNullException(nameof(proposta));
        }

        _context.Propostas.Update(proposta);
        await _context.SaveChangesAsync();
        return proposta;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var proposta = await _context.Propostas.FirstOrDefaultAsync(p => p.PropostaId == id);
        if (proposta is null)
        {
            return false;
        }

        _context.Propostas.Remove(proposta);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Proposta>> GetByStatusAsync(StatusProposta status)
    {
        return await _context.Propostas
            .Where(p => p.Status == status)
            .ToListAsync();
    }

}
