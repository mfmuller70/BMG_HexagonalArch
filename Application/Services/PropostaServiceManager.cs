using Domain.Entities;
using Domain.Ports;
using Infra.Data.Services;

namespace Application.Services;

public class PropostaServiceManager : IPropostaService
{
    private readonly IPropostaRepository _propostaRepository;
    private readonly StatusEventService _statusEventService;

    public PropostaServiceManager(IPropostaRepository propostaRepository, StatusEventService statusEventService)
    {
        _propostaRepository = propostaRepository;
        _statusEventService = statusEventService;
    }

    public async Task<IEnumerable<Proposta>> GetAllPropostasAsync()
    {
        return await _propostaRepository.GetAllAsync();
    }

    public async Task<Proposta?> GetPropostaByIdAsync(Guid id)
    {
        return await _propostaRepository.GetByIdAsync(id);
    }

    public async Task<Proposta> CriarPropostaAsync(string clienteNome, decimal valorCobertura)
    {
        var proposta = new Proposta(clienteNome, valorCobertura);
        var propostaCriada = await _propostaRepository.InsertAsync(proposta);

        return propostaCriada;
    }

    public async Task<Proposta> AtualizarStatusPropostaAsync(Guid id, StatusProposta novoStatus)
    {
        var proposta = await _propostaRepository.GetByIdAsync(id);
        if (proposta == null)
        {
            throw new Exception($"Proposta com ID {id} não encontrada");
        }

        proposta.DefinirStatus(novoStatus);

        var propostaAtualizada = await _propostaRepository.UpdateAsync(proposta);

        return propostaAtualizada;
    }

    public async Task<Proposta> AprovarPropostaAsync(Guid id)
    {
        var proposta = await _propostaRepository.GetByIdAsync(id);
        if (proposta == null)
        {
            throw new Exception($"Proposta com ID {id} não encontrada");
        }

        var statusAnterior = proposta.Status;
        proposta.Aprovar();
        var propostaAprovada = await _propostaRepository.UpdateAsync(proposta);

        await _statusEventService.PublicarMudancaStatusAsync(id, statusAnterior, StatusProposta.Aprovada);

        return propostaAprovada;
    }

    public async Task<IEnumerable<Proposta>> GetPropostasByStatusAsync(StatusProposta status)
    {
        return await _propostaRepository.GetByStatusAsync(status);
    }

}
