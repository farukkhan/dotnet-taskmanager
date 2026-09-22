using Application.Commands;

namespace Application.UseCases.Interfaces;

public interface IDeleteTaskItemUseCase
{
    Task ExecuteAsync(DeleteTaskItemCommand deleteTaskItemCommand, CancellationToken cancellationToken);
}
