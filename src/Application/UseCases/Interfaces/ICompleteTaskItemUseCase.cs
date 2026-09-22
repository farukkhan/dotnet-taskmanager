using Application.Commands;
using Domain.Models;

namespace Application.UseCases.Interfaces;

public interface ICompleteTaskItemUseCase
{
    Task<TaskItem> ExecuteAsync(CompleteTaskItemCommand completeTaskItemCommand, CancellationToken cancellationToken);
}
