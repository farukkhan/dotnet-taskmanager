using Application.Commands;
using Application.Ports;
using Application.UseCases.Interfaces;
using Domain.Models;

namespace Application.UseCases;

internal class CreateTaskItemUseCase(ITaskItemRepository taskItemRepository) : ICreateTaskItemUseCase
{
    public async Task<TaskItem> ExecuteAsync(CreateTaskItemCommand createTaskItemCommand, CancellationToken cancellationToken)
    {
        return await taskItemRepository.CreateAsync(new TaskItem(createTaskItemCommand.Title, createTaskItemCommand.Description));
    }
}
