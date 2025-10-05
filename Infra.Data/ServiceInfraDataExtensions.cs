using Domain.Ports;
using Infra.Data.Adapters;
using Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Data;

public static class ServiceInfraDataExtensions
{

    public static IServiceCollection AddSqlServerService(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SqlServerContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IPropostaRepository, PropostaRepository>();
        services.AddScoped<IContratacaoRepository, ContratacaoRepository>();
        return services;
    }



    public static IServiceCollection AddRabbitMQService(this IServiceCollection services)
    {
        services.AddSingleton<IMessageService, RabbitMQService>();
        return services;
    }
}
