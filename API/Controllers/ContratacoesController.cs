using Application.DTOs;
using Application.Mappers;
using Domain.Entities;
using Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContratacoesController : ControllerBase
{
    private readonly IContratacaoService _contratacaoService;
    private readonly ILogger<ContratacoesController> _logger;

    public ContratacoesController(IContratacaoService contratacaoService, ILogger<ContratacoesController> logger)
    {
        _contratacaoService = contratacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Contratar uma proposta (somente se Aprovada)
    /// </summary>
    /// <param name="dto">Dados da contratação contendo o ID da proposta</param>
    /// <returns>Os dados da contratação realizada com número do contrato gerado</returns>
    /// <remarks>
    /// Realiza a contratação de uma proposta que deve estar com status "Aprovada". Gera automaticamente o número do contrato e atualiza o status para "Contratada".
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> ContratarProposta([FromBody] ContratacaoDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contratacao = await _contratacaoService.ContratarPropostaAsync(dto.PropostaId);
            return Ok(contratacao.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao contratar proposta {PropostaId}: {Message}", dto.PropostaId, ex.Message);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// � Verificar status da proposta
    /// </summary>
    /// <remarks>
    /// ## Funcionalidade:
    /// Consulta se uma proposta existe e retorna informações sobre sua disponibilidade.
    /// 
    /// ## Status das Propostas:
    /// 
    /// | Status | Valor | Descrição | Pode Contratar? |
    /// |--------|-------|-----------|-----------------|
    /// | **EmAnalise** | 1 | Proposta em análise | ❌ Não |
    /// | **Aprovada** | 2 | Proposta aprovada | ✅ Sim |
    /// | **Rejeitada** | 3 | Proposta rejeitada | ❌ Não |
    /// | **Contratada** | 4 | Já contratada | ❌ Não |
    /// 
    /// ## Retorno:
    /// - **   Mensagem = "OK", 
    /// - **   Status = "Contratada",
    /// - **   ContratacaoId = contratacao.PropostaId
    /// 
    /// ## Códigos de Resposta:
    /// - **200 OK**: Proposta encontrada
    /// - **404 Not Found**: Proposta não existe
    /// - **500 Internal Server Error**: Erro interno
    /// </remarks>
    [HttpGet("verificar-status/{propostaId:guid}")]
    public async Task<IActionResult> VerificarStatusProposta(Guid propostaId)
    {
        try
        {
            var proposta = await _contratacaoService.VerificarStatusPropostaAsync(propostaId);
            if (proposta == null)
            {
                return NotFound("Proposta não encontrada");
            }

            // Se a proposta estiver aprovada, contratar ela
            if (proposta.Status == StatusProposta.Aprovada)
            {
                var contratacao = await _contratacaoService.ContratarPropostaAsync(propostaId);
                return Ok(new
                {
                    Mensagem = "OK",
                    Status = "Contratada",
                    ContratacaoId = contratacao.PropostaId
                });
            }

            return Ok(new
            {
                Mensagem = "OK",
                Status = proposta.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao verificar status da proposta {PropostaId}: {Message}", propostaId, ex.Message);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

}