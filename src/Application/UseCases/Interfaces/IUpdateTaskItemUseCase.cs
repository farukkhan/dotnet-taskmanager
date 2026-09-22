using Application.Commands;
using Domain.Models;

namespace Application.UseCases.Interfaces;

public interface IUpdateTaskItemUseCase
{
    Task<TaskItem> ExecuteAsync(UpdateTaskItemCommand createTaskItemCommand, CancellationToken cancellationToken);
}
