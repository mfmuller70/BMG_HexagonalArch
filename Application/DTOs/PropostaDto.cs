namespace Application.DTOs
{
    public class StatusPropostaDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class PropostaDto
    {
        public Guid PropostaId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public decimal ValorCobertura { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DataAtualizacao { get; set; }
    }

    public class CriarPropostaDto
    {
        public string ClienteNome { get; set; } = string.Empty;
        public decimal ValorCobertura { get; set; }
    }

    public class AtualizarStatusPropostaDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
