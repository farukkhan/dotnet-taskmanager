using Application.Commands;
using Application.Queries;
using Application.UseCases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Dtos.Request;
using TaskManager.Api.Mappers.Response;

namespace TaskManager.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskItemController(ICreateTaskItemUseCase createTaskItemUseCase,
        IUpdateTaskItemUseCase updateTaskItemUseCase,
        IGetTaskItemsUseCase getTaskItemsUseCase,
        IGetTaskItemUseCase getTaskItemUseCase,
        IDeleteTaskItemUseCase deleteTaskItemUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTaskItemsAsync(CancellationToken cancellationToken)
    {
        var taskItems = await getTaskItemsUseCase.ExecuteAsync(new GetTaskItemsQuery(), cancellationToken);

        return Ok(taskItems.Select(t => t.ToGetTaskItemResponseDto()));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskItemByIdAsync(int id, CancellationToken cancellationToken)
    {
        var taskItem = await getTaskItemUseCase.ExecuteAsync(new GetTaskItemQuery(id), cancellationToken);

        return taskItem is null ? NotFound($"Task with id:{id} is not found.") : Ok(taskItem.ToGetTaskItemResponseDto());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTaskItemAsync(CreateTaskItemDto createTaskItemDto)
    {
        var taskItem = await createTaskItemUseCase.ExecuteAsync(new CreateTaskItemCommand(createTaskItemDto.Title, createTaskItemDto.Description)
            , CancellationToken.None);

        return CreatedAtAction("GetTaskItemById", new { id = taskItem.Id }, taskItem.ToCreatedTaskItemResponseDto());
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaskItemAsync(int id, UpdateTaskItemDto updateTaskItemDto)
    {
        var taskItem = await updateTaskItemUseCase.ExecuteAsync(new UpdateTaskItemCommand(id, updateTaskItemDto.Title, updateTaskItemDto.Description, updateTaskItemDto.Version), CancellationToken.None);

        return Ok(taskItem.ToUpdatedTaskItemResponseDto());
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTaskItemAsync(int id)
    {
        await deleteTaskItemUseCase.ExecuteAsync(new DeleteTaskItemCommand(id), CancellationToken.None);

        return Ok();
    }
}
