namespace Application.DTOs;

public class CriarContratacaoDto
{
    public Guid PropostaId { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}
