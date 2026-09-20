namespace Domain.Models;

public class TaskItem
{
    public TaskItem(string title, string? description)
    {
        ValidateTitle(title);
        Title = title;

        ValidateDescription(description);
        Description = description;

        CreatedAt = DateTime.UtcNow;
    }

    private TaskItem(int id, string title, string? description, bool isCompleted, DateTime createdAt, DateTime? updatedAt)
    {
        Id= id;

        ValidateTitle(title);
        Title = title;

        ValidateDescription(description);
        Description = description;
        
        IsCompleted = isCompleted;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static TaskItem Load(int id, string title, string? description, bool isCompleted, DateTime createdAt, DateTime? updatedAt)
    {
        return new TaskItem(id, title, description, isCompleted, createdAt, updatedAt);
    }

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void UpdateTitle(string title)
    {
        ValidateTitle(title);

        if (title != Title)
        {
            Title = title;
            UpdateUpdatedAt();
        }
    }

    public void UpdateDescription(string? description)
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
        if (string.IsNullOrEmpty(title) || title.Length > 200)
        {
            throw new ArgumentException("Title must be between 1 and 200 characters.");
        }
    }

    private void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > 2000)
        {
            throw new ArgumentException("Description must be less than or equal to 2000 characters.");
        }
    }

    private void UpdateUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

}
