using Application.Commands;
using Application.UseCases.Interfaces;
using Domain.Models;

namespace Application.UseCases;

public class UpdateTaskItemUseCase : IUpdateTaskItemUseCase
{
    public async Task<TaskItem> ExecuteAsync(UpdateTaskItemCommand createTaskItemCommand, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
