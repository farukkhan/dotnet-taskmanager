using Application.Commands;
using Application.Ports;
using Application.UseCases.Interfaces;

namespace Application.UseCases;

internal class DeleteTaskItemUseCase(ITaskItemRepository taskItemRepository) : IDeleteTaskItemUseCase
{
    public async Task ExecuteAsync(DeleteTaskItemCommand deleteTaskItemCommand, CancellationToken cancellationToken)
    {
        await taskItemRepository.DeleteByIdAsync(deleteTaskItemCommand.Id);
    }
}
