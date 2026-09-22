using Application.Commands;
using Domain.Models;

namespace Application.UseCases.Interfaces;

public interface ICreateTaskItemUseCase
{
    Task<TaskItem> ExecuteAsync(CreateTaskItemCommand createTaskItemCommand, CancellationToken cancellationToken);
}
