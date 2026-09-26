using System.Net;
using System.Net.Http.Json;
using Application.Commands;
using Application.UseCases.Interfaces;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManager.Api.Tests.Validation;

/// <summary>
/// Invalid input must return 400: request DTO validation stops it before the use case runs,
/// and the domain's own validation is mapped from <see cref="DomainValidationException"/>.
/// </summary>
public class ValidationTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    private readonly RecordingCreateTaskItemUseCase _createUseCase = new();
    private readonly RecordingUpdateTaskItemUseCase _updateUseCase = new();

    [Fact]
    public async Task Post_WithEmptyTitle_Returns400ValidationProblemDetails()
    {
        var response = await CreateClient().PostAsJsonAsync("/api/TaskItem", new { Title = "", Description = "Description" });

        await AssertValidationProblemAsync(response, "Title");
        Assert.False(_createUseCase.WasCalled);
    }

    [Fact]
    public async Task Post_WithTooLongTitle_Returns400ValidationProblemDetails()
    {
        var request = new { Title = new string('x', TaskItem.TitleMaxLength + 1), Description = "Description" };

        var response = await CreateClient().PostAsJsonAsync("/api/TaskItem", request);

        await AssertValidationProblemAsync(response, "Title");
        Assert.False(_createUseCase.WasCalled);
    }

    [Fact]
    public async Task Post_WithMaxLengthTitleAndDescription_Returns201()
    {
        var request = new { Title = new string('x', TaskItem.TitleMaxLength), Description = new string('y', TaskItem.DescriptionMaxLength) };

        var response = await CreateClient().PostAsJsonAsync("/api/TaskItem", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(_createUseCase.WasCalled);
    }

    [Fact]
    public async Task Put_WithTooLongDescription_Returns400ValidationProblemDetails()
    {
        var request = new { Title = "Title", Description = new string('x', TaskItem.DescriptionMaxLength + 1), Version = 1 };

        var response = await CreateClient().PutAsJsonAsync("/api/TaskItem/42", request);

        await AssertValidationProblemAsync(response, "Description");
        Assert.False(_updateUseCase.WasCalled);
    }

    [Fact]
    public async Task Put_WhenDomainValidationFails_Returns400ProblemDetails()
    {
        _updateUseCase.ExceptionToThrow = new DomainValidationException("Title must be between 1 and 200 characters.");

        var response = await CreateClient().PutAsJsonAsync("/api/TaskItem/42", new { Title = "Title", Description = "Description", Version = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Equal("Title must be between 1 and 200 characters.", problem.Detail);
    }

    [Fact]
    public async Task Put_WhenUseCaseThrowsArgumentException_Returns500()
    {
        _updateUseCase.ExceptionToThrow = new ArgumentException("An item with the same key has already been added.");

        var response = await CreateClient().PutAsJsonAsync("/api/TaskItem/42", new { Title = "Title", Description = "Description", Version = 1 });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("same key", await response.Content.ReadAsStringAsync());
    }

    private HttpClient CreateClient()
    {
        return factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddScoped<ICreateTaskItemUseCase>(_ => _createUseCase);
            services.AddScoped<IUpdateTaskItemUseCase>(_ => _updateUseCase);
        })).CreateClient();
    }

    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string expectedField)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
        Assert.Contains(problem.Errors.Keys, key => key.Equals(expectedField, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class RecordingCreateTaskItemUseCase : ICreateTaskItemUseCase
    {
        public bool WasCalled { get; private set; }

        public Task<TaskItem> ExecuteAsync(CreateTaskItemCommand createTaskItemCommand, CancellationToken cancellationToken)
        {
            WasCalled = true;
            var taskItem = TaskItem.Load(1, createTaskItemCommand.Title, createTaskItemCommand.Description, false, DateTime.UtcNow, null, 1);
            return Task.FromResult(taskItem);
        }
    }

    private sealed class RecordingUpdateTaskItemUseCase : IUpdateTaskItemUseCase
    {
        public bool WasCalled { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task<TaskItem> ExecuteAsync(UpdateTaskItemCommand updateTaskItemCommand, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return ExceptionToThrow is null
                ? Task.FromResult(TaskItem.Load(updateTaskItemCommand.Id, updateTaskItemCommand.Title, updateTaskItemCommand.Description, false, DateTime.UtcNow, null, 2))
                : Task.FromException<TaskItem>(ExceptionToThrow);
        }
    }
}
