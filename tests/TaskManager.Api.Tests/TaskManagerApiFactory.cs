using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskManager.Api.Tests;

/// <summary>
/// Hosts the API in memory for tests. It runs as Production so tests see what real clients see
/// (no developer exception details, no .env loading).
/// </summary>
public class TaskManagerApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
    }
}
