using Domain.Models;
using Infrastructure.Persistence.Models;

namespace Infrastructure.Persistence.Mappers;

internal static class TaskItemMapper
{
    public static TaskItem ToDomain(this TaskItemEntity taskItemEntity)
    {
        return TaskItem.Load(taskItemEntity.Id, taskItemEntity.Title,
            taskItemEntity.Description, taskItemEntity.IsCompleted, taskItemEntity.CreatedAt, taskItemEntity.UpdatedAt, taskItemEntity.Version);
    }

    public static TaskItemEntity ToEntity(this TaskItem taskItem)
    {
        return new TaskItemEntity
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            IsCompleted = taskItem.IsCompleted,
            CreatedAt = taskItem.CreatedAt,
            UpdatedAt = taskItem.UpdatedAt,
            Version = taskItem.Version
        };
    }
}
