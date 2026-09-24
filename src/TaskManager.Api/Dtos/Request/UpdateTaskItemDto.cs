using System.ComponentModel.DataAnnotations;
using Domain.Models;

namespace TaskManager.Api.Dtos.Request;

public class UpdateTaskItemDto
{
    [Required]
    [StringLength(TaskItem.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [StringLength(TaskItem.DescriptionMaxLength)]
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }

    public int Version { get; set; }
}
