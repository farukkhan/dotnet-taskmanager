using Domain.Exceptions;
using Domain.Models;

namespace Domain.Tests.Models;

public class TaskItemTests
{
    private const int CurrentVersion = 3;

    private static TaskItem LoadTaskItem()
    {
        return TaskItem.Load(1, "Original title", "Original description", false,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, CurrentVersion);
    }

    [Fact]
    public void Update_WithCurrentVersion_UpdatesTitleAndDescription()
    {
        var taskItem = LoadTaskItem();

        taskItem.Update("New title", "New description", CurrentVersion);

        Assert.Equal("New title", taskItem.Title);
        Assert.Equal("New description", taskItem.Description);
        Assert.NotNull(taskItem.UpdatedAt);
    }

    [Theory]
    [InlineData(CurrentVersion - 1)]
    [InlineData(CurrentVersion + 1)]
    public void Update_WithOutdatedVersion_ThrowsVersionConflictException(int expectedVersion)
    {
        var taskItem = LoadTaskItem();

        var exception = Assert.Throws<VersionConflictException>(
            () => taskItem.Update("New title", "New description", expectedVersion));

        Assert.Equal(nameof(TaskItem), exception.EntityName);
        Assert.Equal(1, exception.Id);
        Assert.Equal(expectedVersion, exception.ExpectedVersion);
        Assert.Equal(CurrentVersion, exception.ActualVersion);
    }

    [Fact]
    public void Update_WithOutdatedVersion_LeavesTaskUnchanged()
    {
        var taskItem = LoadTaskItem();

        Assert.Throws<VersionConflictException>(
            () => taskItem.Update("New title", "New description", CurrentVersion - 1));

        Assert.Equal("Original title", taskItem.Title);
        Assert.Equal("Original description", taskItem.Description);
        Assert.Null(taskItem.UpdatedAt);
        Assert.Equal(CurrentVersion, taskItem.Version);
    }

    [Fact]
    public void Update_WithInvalidDescription_ThrowsArgumentException()
    {
        var taskItem = LoadTaskItem();

        Assert.Throws<ArgumentException>(
            () => taskItem.Update("New title", new string('x', 2001), CurrentVersion));
    }
}
