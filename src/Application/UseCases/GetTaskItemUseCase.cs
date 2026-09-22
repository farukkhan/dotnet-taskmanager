using Application.Ports;
using Application.Queries;
using Application.UseCases.Interfaces;
using Domain.Models;

namespace Application.UseCases;

internal class GetTaskItemUseCase(ITaskItemRepository taskItemRepository) : IGetTaskItemUseCase
{
    public async Task<TaskItem?> ExecuteAsync(GetTaskItemQuery getTaskItemQuery, CancellationToken cancellationToken)
    {
        return await taskItemRepository.GetByIdAsync(getTaskItemQuery.Id, cancellationToken);
    }
}
