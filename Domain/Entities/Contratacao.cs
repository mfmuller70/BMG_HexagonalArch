namespace Domain.Entities;

public sealed class Contratacao
{
    public Guid PropostaId { get; private set; }
    public DateTime DataContratacao { get; private set; }
    public string NumeroContrato { get; private set; }

    private Contratacao() { NumeroContrato = string.Empty; }

    public Contratacao(Guid propostaId)
    {
        PropostaId = propostaId;
        DataContratacao = DateTime.UtcNow;
        NumeroContrato = GerarNumeroContrato();
    }

    private static string GerarNumeroContrato()
    {
        return $"CTR{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
