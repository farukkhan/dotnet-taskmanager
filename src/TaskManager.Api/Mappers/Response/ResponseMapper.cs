using Domain.Models;
using TaskManager.Api.Dtos.Response;

namespace TaskManager.Api.Mappers.Response;

public static class ResponseMapper
{
    public static CreatedTaskItemDto ToCreatedTaskItemResponseDto(this TaskItem taskItem)
    {
        return new CreatedTaskItemDto
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            IsCompleted = taskItem.IsCompleted,
            Version = taskItem.Version
        };
    }

    public static GetTaskItemDto ToGetTaskItemResponseDto(this TaskItem taskItem)
    {
        return new GetTaskItemDto
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            IsCompleted = taskItem.IsCompleted,
            Version = taskItem.Version
        };
    }

    public static UpdatedTaskItemDto ToUpdatedTaskItemResponseDto(this TaskItem taskItem)
    {
        return new UpdatedTaskItemDto
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            IsCompleted = taskItem.IsCompleted,
            Version = taskItem.Version
        };
    }
}
