using Application.UseCases;
using Application.UseCases.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICreateTaskItemUseCase, CreateTaskItemUseCase>();
        services.AddScoped<IUpdateTaskItemUseCase, UpdateTaskItemUseCase>();
        services.AddScoped<IGetTaskItemsUseCase, GetTaskItemsUseCase>();
        services.AddScoped<IGetTaskItemUseCase, GetTaskItemUseCase>();
        services.AddScoped<IDeleteTaskItemUseCase, DeleteTaskItemUseCase>();

        return services;
    }
}
