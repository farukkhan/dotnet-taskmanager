namespace TaskManager.Api.Dtos.Request;

public class CreateTaskItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
