using Application.Ports;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence.Adapters;

internal class TaskItemRepository : ITaskItemRepository
{
    private readonly TaskManagerDbContext _taskManagerDbContext;

    public TaskItemRepository(TaskManagerDbContext taskManagerDbContext)
    {
        _taskManagerDbContext = taskManagerDbContext;        
    }

    public async Task<TaskItem> CreateAsync(TaskItem taskItem)
    {
        var taskItemEntity = new TaskItemEntity()
        {
            Title = taskItem.Title,
            Description = taskItem.Description,
            IsCompleted = taskItem.IsCompleted,
            CreatedAt = taskItem.CreatedAt,
            UpdatedAt = taskItem.UpdatedAt
        };

        await _taskManagerDbContext.TaskItemEntities.AddAsync(taskItemEntity);
        await _taskManagerDbContext.SaveChangesAsync();

        return TaskItemMapper.ToDomain(taskItemEntity);
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync()
    {
        return await _taskManagerDbContext.TaskItemEntities.Select(taskItemEntity =>
        TaskItemMapper.ToDomain(taskItemEntity)).AsNoTracking().ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        var taskItemEntity = await _taskManagerDbContext.TaskItemEntities.AsNoTracking().FirstOrDefaultAsync(task => task.Id == id);

        return taskItemEntity is not null ? taskItemEntity.ToDomain() : null;
    }

    public async Task UpdateAsync(TaskItem taskItem)
    {
        var taskItemEntity = taskItem.ToEntity();

        await _taskManagerDbContext.TaskItemEntities.Where(taskEntity => taskEntity.Id == taskItem.Id)
            .ExecuteUpdateAsync(te => te.SetProperty(t => t.Title, taskItem.Title)
            .SetProperty(t => t.Description, taskItem.Description)
            .SetProperty(t => t.IsCompleted, taskItem.IsCompleted)
            .SetProperty(t => t.CreatedAt, taskItem.CreatedAt)
            .SetProperty(t => t.UpdatedAt, taskItem.UpdatedAt));
    }
}
