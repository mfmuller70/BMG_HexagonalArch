using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Seguros.Tests.Unit.Domain.Entities;

public class PropostaTests
{
    [Fact]
    public void Proposta_DeveCriarComDadosValidos()
    {
        var clienteNome = "João Silva";
        var valorCobertura = 100000m;
        var proposta = new Proposta(clienteNome, valorCobertura);

        proposta.ClienteNome.Should().Be(clienteNome);
        proposta.ValorCobertura.Should().Be(valorCobertura);
        proposta.Status.Should().Be(StatusProposta.EmAnalise);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Jo")]
    public void Proposta_DeveLancarExcecaoComNomeInvalido(string clienteNome)
    {
        var action = () => new Proposta(clienteNome, 100000);
        action.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Proposta_DeveLancarExcecaoComValorCoberturaInvalido(decimal valorCobertura)
    {
        var action = () => new Proposta("João Silva", valorCobertura);
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void Aprovar_DeveAlterarStatusParaAprovada()
    {
        var proposta = new Proposta("João Silva", 100000);

        proposta.Aprovar();

        proposta.Status.Should().Be(StatusProposta.Aprovada);
        proposta.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Contratar_DeveAlterarStatusParaContratadaQuandoAprovada()
    {
        var proposta = new Proposta("João Silva", 100000);
        proposta.Aprovar();

        proposta.Contratar();

        proposta.Status.Should().Be(StatusProposta.Contratada);
        proposta.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Contratar_DeveLancarExcecaoQuandoNaoAprovada()
    {
        var proposta = new Proposta("João Silva", 100000);

        var action = () => proposta.Contratar();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Apenas propostas aprovadas podem ser contratadas");
    }

    [Fact]
    public void AtualizarStatus_DeveLancarExcecaoQuandoJaContratada()
    {
        var proposta = new Proposta("João Silva", 100000);
        proposta.Aprovar();
        proposta.Contratar();

        var action = () => proposta.AtualizarStatus(StatusProposta.EmAnalise);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível alterar o status de uma proposta já contratada");
    }

    [Fact]
    public void AtualizarStatus_DeveAlterarStatusCorretamente()
    {
        var proposta = new Proposta("João Silva", 100000);

        proposta.AtualizarStatus(StatusProposta.Aprovada);

        proposta.Status.Should().Be(StatusProposta.Aprovada);
        proposta.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
