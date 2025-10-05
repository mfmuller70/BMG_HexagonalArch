namespace Application.DTOs;

public class ContratacaoDto
{
    public Guid PropostaId { get; set; }
    public DateTime DataContratacao { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
}