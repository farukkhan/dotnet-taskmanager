namespace TaskManager.Api.Dtos.Request;

public class UpdateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }

    public int Version { get; set; }
}
