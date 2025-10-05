using Domain.Entities;
using Domain.Ports;
using Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data.Adapters;

public class ContratacaoRepository : IContratacaoRepository
{
    private readonly SqlServerContext _context;

    public ContratacaoRepository(SqlServerContext context)
    {
        _context = context;
    }

    public async Task<Contratacao?> GetByPropostaIdAsync(Guid propostaId)
    {
        return await _context.Contratacoes
            .FirstOrDefaultAsync(c => c.PropostaId == propostaId);
    }

    public async Task<Contratacao> InsertAsync(Contratacao contratacao)
    {
        if (contratacao is null)
        {
            throw new ArgumentNullException(nameof(contratacao));
        }

        _context.Contratacoes.Add(contratacao);
        await _context.SaveChangesAsync();
        return contratacao;
    }
}
