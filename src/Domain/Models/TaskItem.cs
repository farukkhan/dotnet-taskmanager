using Domain.Exceptions;

namespace Domain.Models;

public class TaskItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public TaskItem(string title, string? description)
    {
        ValidateTitle(title);
        Title = title;

        ValidateDescription(description);
        Description = description;

        CreatedAt = DateTime.UtcNow;
        Version = 1;
    }

    private TaskItem(int id, string title, string? description, bool isCompleted, DateTime createdAt, DateTime? updatedAt, int version)
    {
        Id= id;

        ValidateTitle(title);
        Title = title;

        ValidateDescription(description);
        Description = description;
        
        IsCompleted = isCompleted;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Version = version;
    }

    public static TaskItem Load(int id, string title, string? description, bool isCompleted, DateTime createdAt, DateTime? updatedAt,int version)
    {
        return new TaskItem(id, title, description, isCompleted, createdAt, updatedAt, version);
    }

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public void Update(string title, string? description, int expectedVersion)
    {
        EnsureVersion(expectedVersion);

        UpdateTitle(title);
        UpdateDescription(description);
    }

    private void UpdateTitle(string title)
    {
        ValidateTitle(title);

        if (title != Title)
        {
            Title = title;
            UpdateUpdatedAt();
        }
    }

    private void UpdateDescription(string? description)
    {
        ValidateDescription(description);

        if (description!= Description)
        {
            Description = description;
            UpdateUpdatedAt();
        }
    }

    public void Complete()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            UpdateUpdatedAt();
        }
    }

    public void Reopen()
    {
        if (IsCompleted)
        {
            IsCompleted = false;
            UpdateUpdatedAt();
        }
    }

    private void ValidateTitle(string title)
    {
        if (string.IsNullOrEmpty(title) || title.Length > TitleMaxLength)
        {
            throw new DomainValidationException($"Title must be between 1 and {TitleMaxLength} characters.");
        }
    }

    private void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > DescriptionMaxLength)
        {
            throw new DomainValidationException($"Description must be less than or equal to {DescriptionMaxLength} characters.");
        }
    }

    private void EnsureVersion(int expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new VersionConflictException(nameof(TaskItem), Id, expectedVersion, Version);
        }
    }

    private void UpdateUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

}
