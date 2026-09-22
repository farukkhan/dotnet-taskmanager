using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskManager.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();

builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5216/")
    });

builder.Services.AddScoped<TaskApiClient>();