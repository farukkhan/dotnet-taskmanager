using Application.Commands;
using Application.UseCases.Interfaces;
using Domain.Models;

namespace Application.UseCases;

public class CompleteTaskItemUseCase : ICompleteTaskItemUseCase
{
    public async Task<TaskItem> ExecuteAsync(CompleteTaskItemCommand completeTaskItemCommand, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
