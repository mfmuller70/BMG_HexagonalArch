using Application.DTOs;
using Application.Mappers;
using Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PropostasController : ControllerBase
{
    private readonly IPropostaService _propostaService;
    private readonly IMessageService _messageService;
    private readonly ILogger<PropostasController> _logger;

    public PropostasController(IPropostaService propostaService, IMessageService messageService, ILogger<PropostasController> logger)
    {
        _propostaService = propostaService;
        _messageService = messageService;
        _logger = logger;
    }

    /// <summary>
    /// Listar todas as propostas
    /// </summary>
    /// <returns>Lista de todas as propostas cadastradas no sistema</returns>
    /// <remarks>
    /// Retorna um array com todas as propostas existentes, incluindo informações como ID, nome do cliente, valor da cobertura e status atual.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetAllPropostas()
    {
        try
        {
            var propostas = await _propostaService.GetAllPropostasAsync();
            var propostasDto = propostas.Select(p => p.ToDto());
            return Ok(propostasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter propostas");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Criar uma nova proposta de seguro
    /// </summary>
    /// <param name="dto">Dados para criação da proposta (nome do cliente e valor da cobertura)</param>
    /// <returns>A proposta criada com ID gerado automaticamente</returns>
    /// <remarks>
    /// Cria uma nova proposta com status inicial "EmAnalise". O ID da proposta é gerado automaticamente pelo sistema.
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> CriarProposta([FromBody] CriarPropostaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var proposta = await _propostaService.CriarPropostaAsync(
                dto.ClienteNome,
                dto.ValorCobertura
            );

            return CreatedAtAction(nameof(GetPropostaById), new { id = proposta.PropostaId }, proposta.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar proposta");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obter proposta por ID
    /// </summary>
    /// <param name="id">ID único da proposta a ser consultada</param>
    /// <returns>Os dados completos da proposta solicitada</returns>
    /// <remarks>
    /// Busca uma proposta específica pelo seu ID. Retorna erro 404 se a proposta não for encontrada.
    /// </remarks>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPropostaById(Guid id)
    {
        try
        {
            var proposta = await _propostaService.GetPropostaByIdAsync(id);
            if (proposta == null)
            {
                return NotFound($"Proposta com ID {id} não encontrada");
            }

            return Ok(proposta.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter proposta {PropostaId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// 🔄 Alterar status da proposta
    /// </summary>
    /// <remarks>
    /// ## Status Disponíveis:
    /// 
    /// | Status | Valor | Descrição |
    /// |--------|-------|-----------|
    /// | **EmAnalise** | 1 | Proposta em análise (status inicial) |
    /// | **Aprovada** | 2 | Proposta aprovada para contratação |
    /// | **Rejeitada** | 3 | Proposta rejeitada |
    /// | **Contratada** | 4 | Proposta contratada (status final) |
    /// 
    /// ## Regras de Transição:
    /// - ✅ **EmAnalise** → **Aprovada** ou **Rejeitada**
    /// - ✅ **Aprovada** → **Contratada** (apenas via contratação)
    /// - ❌ **Contratada** → Nenhum outro status (status final)
    /// - ❌ **Rejeitada** → Nenhum outro status
    /// 
    /// ## Retorno:
    ///   "status": "Aprovada"
    /// </remarks>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatusProposta(Guid id, [FromBody] AtualizarStatusPropostaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obter proposta atual para comparar status
            var propostaAtual = await _propostaService.GetPropostaByIdAsync(id);
            if (propostaAtual == null)
            {
                return NotFound($"Proposta com ID {id} não encontrada");
            }

            var statusAnterior = propostaAtual.Status;
            var novoStatus = dto.Status.ToStatusProposta();

            // Atualizar status da proposta (já publica evento automaticamente)
            var proposta = await _propostaService.AtualizarStatusPropostaAsync(id, novoStatus);

            return Ok(proposta.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao atualizar status da proposta {PropostaId}: {Message}", id, ex.Message);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}