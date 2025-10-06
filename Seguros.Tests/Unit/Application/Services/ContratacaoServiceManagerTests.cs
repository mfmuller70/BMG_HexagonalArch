using Application.Services;
using Domain.Entities;
using Domain.Ports;
using FluentAssertions;
using Infra.Data.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Seguros.Tests.Unit.Application.Services;

public class ContratacaoServiceManagerTests
{
    private readonly Mock<IContratacaoRepository> _contratacaoRepositoryMock;
    private readonly Mock<IPropostaService> _propostaServiceMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<ILogger<StatusEventService>> _loggerMock;
    private readonly StatusEventService _statusEventService;
    private readonly ContratacaoServiceManager _service;

    public ContratacaoServiceManagerTests()
    {
        _contratacaoRepositoryMock = new Mock<IContratacaoRepository>();
        _propostaServiceMock = new Mock<IPropostaService>();
        _messageServiceMock = new Mock<IMessageService>();
        _loggerMock = new Mock<ILogger<StatusEventService>>();

        _statusEventService = new StatusEventService(_messageServiceMock.Object, _loggerMock.Object);

        _service = new ContratacaoServiceManager(
            _contratacaoRepositoryMock.Object,
            _propostaServiceMock.Object,
            _statusEventService);
    }

    [Fact]
    public async Task VerificarStatusPropostaAsync_DeveRetornarPropostaQuandoExiste()
    {
        var propostaId = Guid.NewGuid();
    var proposta = new Proposta("João Silva", 100000);
        proposta.Aprovar(); // Status Aprovada

        _propostaServiceMock.Setup(x => x.GetPropostaByIdAsync(propostaId))
            .ReturnsAsync(proposta);

        var result = await _service.VerificarStatusPropostaAsync(propostaId);

        result.Should().NotBeNull();
        result!.ClienteNome.Should().Be("João Silva");
        result.Status.Should().Be(StatusProposta.Aprovada);
        _propostaServiceMock.Verify(x => x.GetPropostaByIdAsync(propostaId), Times.Once);
    }

    [Fact]
    public async Task VerificarStatusPropostaAsync_DeveRetornarPropostaComStatusEmAnalise()
    {
        var propostaId = Guid.NewGuid();
    var proposta = new Proposta("João Silva", 100000);
        // Status permanece EmAnalise

        _propostaServiceMock.Setup(x => x.GetPropostaByIdAsync(propostaId))
            .ReturnsAsync(proposta);

        var result = await _service.VerificarStatusPropostaAsync(propostaId);

        result.Should().NotBeNull();
        result!.ClienteNome.Should().Be("João Silva");
        result.Status.Should().Be(StatusProposta.EmAnalise);
        _propostaServiceMock.Verify(x => x.GetPropostaByIdAsync(propostaId), Times.Once);
    }

    [Fact]
    public async Task VerificarStatusPropostaAsync_DeveRetornarNullQuandoPropostaNaoExiste()
    {
        var propostaId = Guid.NewGuid();
        _propostaServiceMock.Setup(x => x.GetPropostaByIdAsync(propostaId))
            .ReturnsAsync((Proposta?)null);

        var result = await _service.VerificarStatusPropostaAsync(propostaId);

        result.Should().BeNull();
        _propostaServiceMock.Verify(x => x.GetPropostaByIdAsync(propostaId), Times.Once);
    }

    [Fact]
    public async Task ContratarPropostaAsync_DeveContratarPropostaAprovadaComSucesso()
    {
        var propostaId = Guid.NewGuid();
        var proposta = new Proposta("João Silva", 100000);
        proposta.Aprovar(); // Status Aprovada

        var contratacao = new Contratacao(propostaId);

        _propostaServiceMock.Setup(x => x.GetPropostaByIdAsync(propostaId))
            .ReturnsAsync(proposta);
        _contratacaoRepositoryMock.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync((Contratacao?)null);
        _contratacaoRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<Contratacao>()))
            .ReturnsAsync(contratacao);

        var result = await _service.ContratarPropostaAsync(propostaId);

        result.Should().NotBeNull();
        result.contratacao.Should().NotBeNull();
        result.contratacao.PropostaId.Should().Be(propostaId);
        result.jaExistia.Should().BeFalse();
        _propostaServiceMock.Verify(x => x.GetPropostaByIdAsync(propostaId), Times.Once);
        _contratacaoRepositoryMock.Verify(x => x.GetByPropostaIdAsync(propostaId), Times.Once);
        _contratacaoRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Contratacao>()), Times.Once);
    }

    [Fact]
    public async Task ContratarPropostaAsync_DeveRetornarContratacaoExistenteQuandoJaExiste()
    {
        var propostaId = Guid.NewGuid();
        var proposta = new Proposta("João Silva", 100000);
        proposta.Aprovar(); // Status Aprovada

        var contratacaoExistente = new Contratacao(propostaId);

        _propostaServiceMock.Setup(x => x.GetPropostaByIdAsync(propostaId))
            .ReturnsAsync(proposta);
        _contratacaoRepositoryMock.Setup(x => x.GetByPropostaIdAsync(propostaId))
            .ReturnsAsync(contratacaoExistente);

        var result = await _service.ContratarPropostaAsync(propostaId);

        result.Should().NotBeNull();
        result.contratacao.Should().BeSameAs(contratacaoExistente);
        result.jaExistia.Should().BeTrue();
        _propostaServiceMock.Verify(x => x.GetPropostaByIdAsync(propostaId), Times.Once);
        _contratacaoRepositoryMock.Verify(x => x.GetByPropostaIdAsync(propostaId), Times.Once);
        _contratacaoRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Contratacao>()), Times.Never);
    }

    [Fact]
    public async Task ContratarPropostaAsync_DeveLancarExcecaoQuandoPropostaNaoExiste()
    {
        var propostaId = Guid.NewGuid();
        _propostaServiceMock.Setup(x => x.GetPropostaByIdAsync(propostaId))
            .ReturnsAsync((Proposta?)null);

        var action = async () => await _service.ContratarPropostaAsync(propostaId);
        await action.Should().ThrowAsync<Exception>()
            .WithMessage($"Proposta com ID {propostaId} não encontrada");
    }

    [Fact]
    public async Task ContratarPropostaAsync_DeveLancarExcecaoQuandoPropostaNaoEstaAprovada()
    {
        var propostaId = Guid.NewGuid();
    var proposta = new Proposta("João Silva", 100000);
        // Status permanece EmAnalise

        _propostaServiceMock.Setup(x => x.GetPropostaByIdAsync(propostaId))
            .ReturnsAsync(proposta);

        var action = async () => await _service.ContratarPropostaAsync(propostaId);
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Apenas propostas aprovadas podem ser contratadas. Status atual: EmAnalise");
    }
}
