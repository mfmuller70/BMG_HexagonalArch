using Domain.Entities;
using Domain.Ports;
using Infra.Data.Services;

namespace Application.Services;

public class ContratacaoServiceManager : IContratacaoService
{
    private readonly IContratacaoRepository _contratacaoRepository;
    private readonly IPropostaService _propostaService;
    private readonly StatusEventService _statusEventService;

    public ContratacaoServiceManager(
        IContratacaoRepository contratacaoRepository,
        IPropostaService propostaService,
        StatusEventService statusEventService)
    {
        _contratacaoRepository = contratacaoRepository;
        _propostaService = propostaService;
        _statusEventService = statusEventService;
    }

    public async Task<(Contratacao contratacao, bool jaExistia)> ContratarPropostaAsync(Guid propostaId)
    {
        var proposta = await _propostaService.GetPropostaByIdAsync(propostaId);
        if (proposta == null)
        {
            throw new Exception($"Proposta com ID {propostaId} não encontrada");
        }

        if (proposta.Status != StatusProposta.Aprovada)
        {
            throw new Exception($"Apenas propostas aprovadas podem ser contratadas. Status atual: {proposta.Status}");
        }

        var contratacaoExistente = await _contratacaoRepository.GetByPropostaIdAsync(propostaId);
        if (contratacaoExistente != null)
        {
            return (contratacaoExistente, true);
        }

        var contratacao = new Contratacao(propostaId);
        var contratacaoCriada = await _contratacaoRepository.InsertAsync(contratacao);

        await _statusEventService.PublicarMudancaStatusAsync(propostaId, StatusProposta.Aprovada, StatusProposta.Contratada);

        return (contratacaoCriada, false);
    }

    public async Task<Proposta?> VerificarStatusPropostaAsync(Guid propostaId)
    {
        return await _propostaService.GetPropostaByIdAsync(propostaId);
    }

}
