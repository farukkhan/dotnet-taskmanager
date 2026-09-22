using Application.Ports;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Adapters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<TaskManagerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITaskItemRepository, TaskItemRepository>();

        return services;
    }
}
