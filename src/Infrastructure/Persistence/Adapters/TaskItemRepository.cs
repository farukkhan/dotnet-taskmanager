using Application.Exceptions;
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
            UpdatedAt = taskItem.UpdatedAt,
            Version = taskItem.Version,
        };

        await _taskManagerDbContext.TaskItemEntities.AddAsync(taskItemEntity);
        await _taskManagerDbContext.SaveChangesAsync();

        return TaskItemMapper.ToDomain(taskItemEntity);
    }

    public async Task DeleteByIdAsync(int id)
    {
        var rowsAffected = await _taskManagerDbContext.TaskItemEntities.Where(t => t.Id == id).ExecuteDeleteAsync();

        if (rowsAffected == 0)
        {
            throw new NotFoundException(
            $"Task with Id {id} was not found.");
        }
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _taskManagerDbContext.TaskItemEntities.Select(taskItemEntity =>
        TaskItemMapper.ToDomain(taskItemEntity)).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var taskItemEntity = await _taskManagerDbContext.TaskItemEntities.AsNoTracking().FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

        return taskItemEntity is not null ? taskItemEntity.ToDomain() : null;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem taskItem)
    {
        var taskItemEntity = await _taskManagerDbContext.TaskItemEntities
            .Where(t => t.Id == taskItem.Id).SingleOrDefaultAsync();

        if (taskItemEntity is null)
        {
            throw new NotFoundException(
            $"Task with Id {taskItem.Id} was not found.");
        }

        if(taskItemEntity.Version != taskItem.Version)
        {
            throw new ConcurrencyException(
           $"Task with Id {taskItem.Id} was modified by another user.");
        }

        taskItemEntity.Title = taskItem.Title;
        taskItemEntity.Description = taskItem.Description;
        taskItemEntity.IsCompleted = taskItem.IsCompleted;
        taskItemEntity.UpdatedAt = taskItem.UpdatedAt;
        taskItemEntity.Version = taskItem.Version + 1;

        try
        {
            await _taskManagerDbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(
            $"Task with Id {taskItem.Id} was modified by another user.");
        }

        return taskItemEntity.ToDomain();


        //await _taskManagerDbContext.TaskItemEntities.Where(taskEntity => taskEntity.Id == taskItem.Id)
        //    .ExecuteUpdateAsync(te => te.SetProperty(t => t.Title, taskItem.Title)
        //    .SetProperty(t => t.Description, taskItem.Description)
        //    .SetProperty(t => t.IsCompleted, taskItem.IsCompleted)
        //    .SetProperty(t => t.CreatedAt, taskItem.CreatedAt)
        //    .SetProperty(t => t.UpdatedAt, taskItem.UpdatedAt));
    }
}
