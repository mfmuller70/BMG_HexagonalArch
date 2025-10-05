namespace Domain.Entities;

public sealed class Proposta
{
    public Guid PropostaId { get; private set; }
    public string ClienteNome { get; private set; }
    public decimal ValorCobertura { get; private set; }
    public StatusProposta Status { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    private Proposta()
    {
        ClienteNome = string.Empty;
        ValorCobertura = 0;
        Status = StatusProposta.EmAnalise;
        DataAtualizacao = DateTime.UtcNow;
        PropostaId = Guid.NewGuid();
    }

    public Proposta(string clienteNome, decimal valorCobertura)
    {
        Validate(clienteNome, valorCobertura);
        PropostaId = Guid.NewGuid();
        ClienteNome = clienteNome;
        ValorCobertura = valorCobertura;
        Status = StatusProposta.EmAnalise;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarStatus(StatusProposta novoStatus)
    {
        if (Status == StatusProposta.Contratada)
        {
            throw new InvalidOperationException("Não é possível alterar o status de uma proposta já contratada");
        }

        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Aprovar()
    {
        AtualizarStatus(StatusProposta.Aprovada);
    }


    public void Contratar()
    {
        if (Status != StatusProposta.Aprovada)
        {
            throw new InvalidOperationException("Apenas propostas aprovadas podem ser contratadas");
        }
        AtualizarStatus(StatusProposta.Contratada);
    }

    public void DefinirStatus(StatusProposta novoStatus)
    {
        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;
    }

    private static void Validate(string clienteNome, decimal valorCobertura)
    {
        if (string.IsNullOrWhiteSpace(clienteNome) || clienteNome.Length < 3)
            throw new InvalidOperationException("Nome do cliente inválido");

        if (valorCobertura <= 0)
            throw new InvalidOperationException("Valor da cobertura deve ser maior que zero");
    }
}

public enum StatusProposta
{
    EmAnalise = 1,
    Aprovada = 2,
    Rejeitada = 3,
    Contratada = 4
}
