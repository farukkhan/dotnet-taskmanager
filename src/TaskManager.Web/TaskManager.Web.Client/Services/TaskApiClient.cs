using System.Net.Http.Json;
using TaskManager.Web.Client.Models;

namespace TaskManager.Web.Client.Services;

public class TaskApiClient(HttpClient httpClient)
{
    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<List<TaskItem>>("api/taskitem")
               ?? [];
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<TaskItem>($"api/taskitem/{id}");
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        var response = await httpClient.PostAsJsonAsync("api/taskitem", task);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TaskItem>()
               ?? throw new InvalidOperationException("Invalid API response.");
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/taskitem/{task.Id}",
            task);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TaskItem>()
               ?? throw new InvalidOperationException("Invalid API response.");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/taskitem/{id}");

        response.EnsureSuccessStatusCode();
    }
}