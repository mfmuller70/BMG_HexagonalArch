using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class PropostaMapper
{
    public static PropostaDto ToDto(this Proposta proposta)
    {
        return new PropostaDto
        {
            PropostaId = proposta.PropostaId,
            ClienteNome = proposta.ClienteNome,
            ValorCobertura = proposta.ValorCobertura,
            Status = proposta.Status.ToString(),
            DataAtualizacao = proposta.DataAtualizacao
        };
    }

    public static Proposta ToEntity(this CriarPropostaDto dto)
    {
        var proposta = new Proposta(
            dto.ClienteNome,
            dto.ValorCobertura
        );
        return proposta;
    }

    public static StatusProposta ToStatusProposta(this string status)
    {
        return Enum.TryParse<StatusProposta>(status, true, out var result)
            ? result
            : throw new ArgumentException($"Status inválido: {status}");
    }
}
