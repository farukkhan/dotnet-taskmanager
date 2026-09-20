using Domain.Models;

namespace Application.Ports;

public interface ITaskItemRepository
{
    Task<TaskItem> CreateAsync(TaskItem taskItem);
    Task<IReadOnlyList<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task UpdateAsync(TaskItem taskItem);
}
