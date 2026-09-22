using Application.Queries;
using Domain.Models;

namespace Application.UseCases.Interfaces;

public interface IGetTaskItemUseCase
{
    Task<TaskItem?> ExecuteAsync(GetTaskItemQuery getTaskItemQuery, CancellationToken cancellationToken);
}
