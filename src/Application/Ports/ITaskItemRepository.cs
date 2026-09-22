using Domain.Models;

namespace Application.Ports;

public interface ITaskItemRepository
{
    Task<TaskItem> CreateAsync(TaskItem taskItem, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TaskItem> UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken);
    Task DeleteByIdAsync(int id, CancellationToken cancellationToken);
}
