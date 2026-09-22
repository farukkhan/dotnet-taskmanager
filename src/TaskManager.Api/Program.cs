using Application;
using DotNetEnv;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Load .env only in local Development environment
if (builder.Environment.IsDevelopment())
{
    Env.Load(); // Looks for a .env file in the project root directory
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
