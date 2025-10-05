using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class ContratacaoMapper
{
    public static ContratacaoDto ToDto(this Contratacao contratacao)
    {
        return new ContratacaoDto
        {
            PropostaId = contratacao.PropostaId,
            DataContratacao = contratacao.DataContratacao,
            NumeroContrato = contratacao.NumeroContrato
        };
    }
}
