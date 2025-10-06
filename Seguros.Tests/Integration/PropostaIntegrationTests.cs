using Application.Services;
using Domain.Entities;
using Domain.Ports;
using FluentAssertions;
using Infra.Data.Adapters;
using Infra.Data.Context;
using Infra.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Seguros.Tests.Integration;

public class PropostaIntegrationTests : IDisposable
{
    private readonly SqlServerContext _context;
    private readonly PropostaServiceManager _service;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<ILogger<StatusEventService>> _loggerMock;
    private readonly StatusEventService _statusEventService;

    public PropostaIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<SqlServerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SqlServerContext(options);
        _messageServiceMock = new Mock<IMessageService>();
        _loggerMock = new Mock<ILogger<StatusEventService>>();

        _statusEventService = new StatusEventService(_messageServiceMock.Object, _loggerMock.Object);

        var propostaRepository = new PropostaRepository(_context);
        var contratacaoRepository = new ContratacaoRepository(_context);
        _service = new PropostaServiceManager(propostaRepository, contratacaoRepository, _statusEventService);
    }

    [Fact]
    public async Task CriarProposta_DevePersistirNoBancoDeDados()
    {
        // Act
    var proposta = await _service.CriarPropostaAsync("João Silva", 100000);

        // Assert
        proposta.Should().NotBeNull();
        proposta.ClienteNome.Should().Be("João Silva");
        proposta.ValorCobertura.Should().Be(100000);
        proposta.Status.Should().Be(StatusProposta.EmAnalise);

        // Verificar se foi persistido no banco
        var propostaNoBanco = await _context.Propostas.FindAsync(proposta.PropostaId);
        propostaNoBanco.Should().NotBeNull();
        propostaNoBanco!.ClienteNome.Should().Be("João Silva");
        propostaNoBanco.Should().NotBeNull();
        propostaNoBanco!.ClienteNome.Should().Be("João Silva");
    }

    [Fact]
    public async Task AprovarProposta_DeveAtualizarStatusNoBancoDeDados()
    {
        // Arrange
    var proposta = await _service.CriarPropostaAsync("João Silva", 100000);

        // Act
        var propostaAprovada = await _service.AprovarPropostaAsync(proposta.PropostaId);

        // Assert
        propostaAprovada.Status.Should().Be(StatusProposta.Aprovada);
        propostaAprovada.DataAtualizacao.Should().NotBeNull();

        // Verificar se foi atualizado no banco
        var propostaNoBanco = await _context.Propostas.FindAsync(proposta.PropostaId);
        propostaNoBanco!.Status.Should().Be(StatusProposta.Aprovada);
        propostaNoBanco.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPropostasByStatus_DeveRetornarPropostasFiltradas()
    {
        // Arrange
    await _service.CriarPropostaAsync("João Silva", 100000);
    var proposta2 = await _service.CriarPropostaAsync("Ana Costa", 50000);
        await _service.AprovarPropostaAsync(proposta2.PropostaId);

        // Act
        var propostasAprovadas = await _service.GetPropostasByStatusAsync(StatusProposta.Aprovada);
        var propostasEmAnalise = await _service.GetPropostasByStatusAsync(StatusProposta.EmAnalise);

        // Assert
        propostasAprovadas.Should().HaveCount(1);
        propostasAprovadas.First().ClienteNome.Should().Be("Ana Costa");
        propostasEmAnalise.Should().HaveCount(1);
        propostasEmAnalise.First().ClienteNome.Should().Be("João Silva");
    }


    [Fact]
    public async Task AtualizarStatusProposta_DeveAtualizarStatusCorretamente()
    {
        // Arrange
    var proposta = await _service.CriarPropostaAsync("João Silva", 100000);

        // Act
        var propostaAtualizada = await _service.AtualizarStatusPropostaAsync(proposta.PropostaId, StatusProposta.Aprovada);

        // Assert
        propostaAtualizada.Status.Should().Be(StatusProposta.Aprovada);
        propostaAtualizada.DataAtualizacao.Should().NotBeNull();

        // Verificar no banco
        var propostaNoBanco = await _context.Propostas.FindAsync(proposta.PropostaId);
        propostaNoBanco!.Status.Should().Be(StatusProposta.Aprovada);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
