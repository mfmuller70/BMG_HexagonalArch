using Application.Services;
using Domain.Entities;
using Domain.Ports;
using FluentAssertions;
using Infra.Data.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Seguros.Tests.Unit.Application.Services;

public class PropostaServiceManagerTests
{
    private readonly Mock<IPropostaRepository> _propostaRepositoryMock;
    private readonly Mock<IContratacaoRepository> _contratacaoRepositoryMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<ILogger<StatusEventService>> _loggerMock;
    private readonly StatusEventService _statusEventService;
    private readonly PropostaServiceManager _service;

    public PropostaServiceManagerTests()
    {
        _propostaRepositoryMock = new Mock<IPropostaRepository>();
        _contratacaoRepositoryMock = new Mock<IContratacaoRepository>();
        _messageServiceMock = new Mock<IMessageService>();
        _loggerMock = new Mock<ILogger<StatusEventService>>();

        _statusEventService = new StatusEventService(_messageServiceMock.Object, _loggerMock.Object);

        _service = new PropostaServiceManager(
            _propostaRepositoryMock.Object,
            _contratacaoRepositoryMock.Object,
            _statusEventService);
    }

    [Fact]
    public async Task CriarPropostaAsync_DeveCriarPropostaComSucesso()
    {
    var proposta = new Proposta("João Silva", 100000);
        _propostaRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<Proposta>()))
            .ReturnsAsync(proposta);

    var result = await _service.CriarPropostaAsync("João Silva", 100000);

        result.Should().NotBeNull();
        result.ClienteNome.Should().Be("João Silva");
        _propostaRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Proposta>()), Times.Once);
    }

    [Fact]
    public async Task AprovarPropostaAsync_DeveAprovarPropostaComSucesso()
    {
    var proposta = new Proposta("João Silva", 100000);
        _propostaRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(proposta);
        _propostaRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Proposta>()))
            .ReturnsAsync(proposta);

        var result = await _service.AprovarPropostaAsync(proposta.PropostaId);

        result.Status.Should().Be(StatusProposta.Aprovada);
        _propostaRepositoryMock.Verify(x => x.GetByIdAsync(proposta.PropostaId), Times.Once);
        _propostaRepositoryMock.Verify(x => x.UpdateAsync(proposta), Times.Once);
    }

    [Fact]
    public async Task AprovarPropostaAsync_DeveLancarExcecaoQuandoPropostaNaoEncontrada()
    {
        var propostaId = Guid.NewGuid();
        _propostaRepositoryMock.Setup(x => x.GetByIdAsync(propostaId))
            .ReturnsAsync((Proposta?)null);

        var action = async () => await _service.AprovarPropostaAsync(propostaId);
        await action.Should().ThrowAsync<Exception>()
            .WithMessage($"Proposta com ID {propostaId} não encontrada");
    }


    [Fact]
    public async Task GetAllPropostasAsync_DeveRetornarTodasAsPropostas()
    {
        var propostas = new List<Proposta>
        {
            new("João Silva", 100000),
            new("Ana Costa", 50000)
        };
        _propostaRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(propostas);

        var result = await _service.GetAllPropostasAsync();

        result.Should().HaveCount(2);
        _propostaRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPropostasByStatusAsync_DeveRetornarPropostasFiltradas()
    {
        var propostas = new List<Proposta>
        {
            new("João Silva", 100000)
        };
        _propostaRepositoryMock.Setup(x => x.GetByStatusAsync(StatusProposta.EmAnalise))
            .ReturnsAsync(propostas);

        var result = await _service.GetPropostasByStatusAsync(StatusProposta.EmAnalise);

        result.Should().HaveCount(1);
        result.First().ClienteNome.Should().Be("João Silva");
        _propostaRepositoryMock.Verify(x => x.GetByStatusAsync(StatusProposta.EmAnalise), Times.Once);
    }

    [Fact]
    public async Task AtualizarStatusPropostaAsync_DeveLancarExcecaoQuandoPropostaJaContratada()
    {
        var propostaId = Guid.NewGuid();
        var proposta = new Proposta("João Silva", 100000);
        var contratacaoExistente = new Contratacao(propostaId);

        _propostaRepositoryMock.Setup(x => x.GetByIdAsync(propostaId))
            .ReturnsAsync(proposta);
        _contratacaoRepositoryMock.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync(contratacaoExistente);

        var act = async () => await _service.AtualizarStatusPropostaAsync(propostaId, StatusProposta.Rejeitada);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Não é possível alterar o status de uma proposta já contratada");
        
        _propostaRepositoryMock.Verify(x => x.GetByIdAsync(propostaId), Times.Once);
        _contratacaoRepositoryMock.Verify(x => x.GetByPropostaIdAsync(propostaId), Times.Once);
        _propostaRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Proposta>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarStatusPropostaAsync_DeveAtualizarQuandoPropostaNaoContratada()
    {
        var propostaId = Guid.NewGuid();
        var proposta = new Proposta("João Silva", 100000);

        _propostaRepositoryMock.Setup(x => x.GetByIdAsync(propostaId))
            .ReturnsAsync(proposta);
        _contratacaoRepositoryMock.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);
        _propostaRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Proposta>()))
            .ReturnsAsync(proposta);

        var result = await _service.AtualizarStatusPropostaAsync(propostaId, StatusProposta.Rejeitada);

        result.Should().NotBeNull();
        result.Status.Should().Be(StatusProposta.Rejeitada);
        _propostaRepositoryMock.Verify(x => x.GetByIdAsync(propostaId), Times.Once);
        _contratacaoRepositoryMock.Verify(x => x.GetByPropostaIdAsync(propostaId), Times.Once);
        _propostaRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Proposta>()), Times.Once);
    }
}
