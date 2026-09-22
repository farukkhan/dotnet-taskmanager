using Application.Commands;
using Application.Exceptions;
using Application.Ports;
using Application.UseCases.Interfaces;
using Domain.Models;

namespace Application.UseCases;

public class UpdateTaskItemUseCase(ITaskItemRepository taskItemRepository) : IUpdateTaskItemUseCase
{
    public async Task<TaskItem> ExecuteAsync(UpdateTaskItemCommand updateTaskItemCommand, CancellationToken cancellationToken)
    {
        var existingTaskItem = await taskItemRepository.GetByIdAsync(updateTaskItemCommand.Id, cancellationToken);

        if (existingTaskItem is null)
        {
            throw new NotFoundException($"Task with id {updateTaskItemCommand.Id} is not found.");
        }

        existingTaskItem.UpdateTitle(updateTaskItemCommand.Title);
        existingTaskItem.UpdateDescription(updateTaskItemCommand.Description);

        return await taskItemRepository.UpdateAsync(existingTaskItem, cancellationToken);
    }
}
