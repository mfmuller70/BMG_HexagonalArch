using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data.Context;

public class SqlServerContext : DbContext
{
    public SqlServerContext(DbContextOptions<SqlServerContext> options)
      : base(options)
    { }

    public DbSet<Proposta> Propostas { get; set; }
    public DbSet<Contratacao> Contratacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Proposta
        modelBuilder.Entity<Proposta>(entity =>
        {
            entity.HasKey(e => e.PropostaId);
            entity.Property(e => e.PropostaId)
                  .ValueGeneratedOnAdd(); // Configura como Identity
            entity.Property(e => e.ClienteNome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ValorCobertura).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.DataAtualizacao).IsRequired();
        });

        // Configuração da entidade Contratacao
        modelBuilder.Entity<Contratacao>(entity =>
        {
            entity.HasKey(e => e.PropostaId);
            entity.Property(e => e.NumeroContrato).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DataContratacao).IsRequired();

            // Relacionamento com Proposta
            entity.HasOne<Proposta>()
                  .WithMany()
                  .HasForeignKey(e => e.PropostaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
