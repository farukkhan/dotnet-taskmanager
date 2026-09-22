using Application.Commands;
using Application.UseCases.Interfaces;
using Domain.Models;

namespace Application.UseCases;

public class ReopenTaskItemUseCase : IReopenTaskItemUseCase
{
    public async Task<TaskItem> ExecuteAsync(ReopenTaskItemCommand reopenTaskItemCommand, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
