using Application.Services;
using Domain.Ports;
using Infra.Data;
using Infra.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceApplicationExtensions
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services, string connectionString)
    {
        services.AddSqlServerService(connectionString);
        services.AddRabbitMQService();

        services.AddTransient<IPropostaService, PropostaServiceManager>();
        services.AddTransient<IContratacaoService, ContratacaoServiceManager>();

        services.AddScoped<StatusEventService>();

        return services;
    }
}
