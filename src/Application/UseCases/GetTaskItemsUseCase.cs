using Application.Ports;
using Application.Queries;
using Application.UseCases.Interfaces;
using Domain.Models;

namespace Application.UseCases;

internal class GetTaskItemsUseCase(ITaskItemRepository taskItemRepository) : IGetTaskItemsUseCase
{
    public async Task<IReadOnlyList<TaskItem>> ExecuteAsync(GetTaskItemsQuery getTaskItemsQuery, CancellationToken cancellationToken)
    {
        return await taskItemRepository.GetAllAsync(cancellationToken);
    }
}
