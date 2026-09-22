using Application.Queries;
using Domain.Models;

namespace Application.UseCases.Interfaces;

public interface IGetTaskItemsUseCase
{
    Task<IReadOnlyList<TaskItem>> ExecuteAsync(GetTaskItemsQuery getTaskItemsQuery, CancellationToken cancellationToken);
}
