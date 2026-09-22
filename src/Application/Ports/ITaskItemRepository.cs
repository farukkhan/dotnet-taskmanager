using Domain.Models;

namespace Application.Ports;

public interface ITaskItemRepository
{
    Task<TaskItem> CreateAsync(TaskItem taskItem);
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TaskItem> UpdateAsync(TaskItem taskItem);
    Task DeleteByIdAsync(int id);
}
