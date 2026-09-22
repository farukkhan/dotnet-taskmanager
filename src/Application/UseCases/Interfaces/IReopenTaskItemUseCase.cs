using Application.Commands;
using Domain.Models;

namespace Application.UseCases.Interfaces;

public interface IReopenTaskItemUseCase
{
    Task<TaskItem> ExecuteAsync(ReopenTaskItemCommand reopenTaskItemCommand, CancellationToken cancellationToken);
}
