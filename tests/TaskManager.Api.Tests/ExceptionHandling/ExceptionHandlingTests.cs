using System.Net;
using System.Net.Http.Json;
using Application.Commands;
using Application.Exceptions;
using Application.Queries;
using Application.UseCases.Interfaces;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManager.Api.Tests.ExceptionHandling;

/// <summary>
/// Runs the real HTTP pipeline with stubbed use cases that throw, so the tests check the
/// exception-to-status mapping without needing a database.
/// </summary>
public class ExceptionHandlingTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    private static readonly object ValidUpdateRequest = new { Title = "Title", Description = "Description", Version = 1 };

    [Fact]
    public async Task GetById_WhenTaskDoesNotExist_Returns404ProblemDetails()
    {
        var client = CreateClient(services => services.AddScoped<IGetTaskItemUseCase, NotFoundGetTaskItemUseCase>());

        var response = await client.GetAsync("/api/TaskItem/42");

        var problem = await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound);
        Assert.Equal("Task with id:42 is not found.", problem.Detail);
    }

    [Fact]
    public async Task Put_WhenUseCaseThrowsNotFound_Returns404ProblemDetails()
    {
        var client = CreateClientWithThrowingUpdate(new NotFoundException("Task with id 42 is not found."));

        var response = await client.PutAsJsonAsync("/api/TaskItem/42", ValidUpdateRequest);

        var problem = await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound);
        Assert.Equal("Task with id 42 is not found.", problem.Detail);
    }

    [Fact]
    public async Task Delete_WhenUseCaseThrowsNotFound_Returns404ProblemDetails()
    {
        var client = CreateClient(services =>
            services.AddScoped<IDeleteTaskItemUseCase>(_ => new ThrowingDeleteTaskItemUseCase(new NotFoundException("Task with Id 42 was not found."))));

        var response = await client.DeleteAsync("/api/TaskItem/42");

        await AssertProblemDetailsAsync(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_WhenVersionConflict_Returns409ProblemDetails()
    {
        var client = CreateClientWithThrowingUpdate(new VersionConflictException(nameof(TaskItem), 42, 1, 2));

        var response = await client.PutAsJsonAsync("/api/TaskItem/42", ValidUpdateRequest);

        var problem = await AssertProblemDetailsAsync(response, HttpStatusCode.Conflict);
        Assert.Contains("modified by another user", problem.Detail);
    }

    [Fact]
    public async Task Put_WhenConcurrencyException_Returns409ProblemDetails()
    {
        var client = CreateClientWithThrowingUpdate(new ConcurrencyException("Task with Id 42 was modified by another user."));

        var response = await client.PutAsJsonAsync("/api/TaskItem/42", ValidUpdateRequest);

        await AssertProblemDetailsAsync(response, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_WhenUnexpectedException_Returns500WithoutExceptionDetails()
    {
        var client = CreateClientWithThrowingUpdate(new InvalidOperationException("Sensitive internal detail"));

        var response = await client.PutAsJsonAsync("/api/TaskItem/42", ValidUpdateRequest);

        var body = await response.Content.ReadAsStringAsync();
        await AssertProblemDetailsAsync(response, HttpStatusCode.InternalServerError);
        Assert.DoesNotContain("Sensitive internal detail", body);
        Assert.DoesNotContain(nameof(InvalidOperationException), body);
        Assert.DoesNotContain("   at ", body);
    }

    private HttpClient CreateClient(Action<IServiceCollection> configureServices)
    {
        return factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(configureServices)).CreateClient();
    }

    private HttpClient CreateClientWithThrowingUpdate(Exception exception)
    {
        return CreateClient(services => services.AddScoped<IUpdateTaskItemUseCase>(_ => new ThrowingUpdateTaskItemUseCase(exception)));
    }

    private static async Task<ProblemDetails> AssertProblemDetailsAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal((int)expectedStatusCode, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));

        return problem;
    }

    private sealed class NotFoundGetTaskItemUseCase : IGetTaskItemUseCase
    {
        public Task<TaskItem?> ExecuteAsync(GetTaskItemQuery getTaskItemQuery, CancellationToken cancellationToken)
            => Task.FromResult<TaskItem?>(null);
    }

    private sealed class ThrowingUpdateTaskItemUseCase(Exception exception) : IUpdateTaskItemUseCase
    {
        public Task<TaskItem> ExecuteAsync(UpdateTaskItemCommand updateTaskItemCommand, CancellationToken cancellationToken)
            => Task.FromException<TaskItem>(exception);
    }

    private sealed class ThrowingDeleteTaskItemUseCase(Exception exception) : IDeleteTaskItemUseCase
    {
        public Task ExecuteAsync(DeleteTaskItemCommand deleteTaskItemCommand, CancellationToken cancellationToken)
            => Task.FromException(exception);
    }
}
