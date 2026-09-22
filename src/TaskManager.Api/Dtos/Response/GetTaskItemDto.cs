namespace TaskManager.Api.Dtos.Response;

public class GetTaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }

    public int Version { get; set; }
}
