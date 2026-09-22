namespace Application.Commands;

public record UpdateTaskItemCommand(int Id, string Title, string? Description, int Version);
