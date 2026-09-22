# Developer Guidelines: Managing EF Core Migrations & Local Docker Database

Welcome to the team! This guideline covers the standard workflow for setting up your local database using Docker, generating Entity Framework Core migrations, running custom SQL migrations, and applying them during local development.

---

## 1. Prerequisites & Required Tools

Before managing migrations locally, ensure you have the required CLI tools and NuGet packages installed.

### Global CLI Tool
Install the EF Core CLI tool globally on your machine:

```powershell
dotnet tool install --global dotnet-ef
To verify the installation, run:

PowerShell
dotnet ef --version
Required NuGet Package
The startup API project requires the EF Core Design package to allow CLI tools to discover DbContext models and build migration snapshots.

Required Package: Microsoft.EntityFrameworkCore.Design

Target Project: src/TaskManager.Api

To install it in the startup project, run:

PowerShell
dotnet add src/TaskManager.Api package Microsoft.EntityFrameworkCore.Design

2. Local Database Setup (Docker)
Our local development environment uses Docker Compose to host a PostgreSQL database container.

docker-compose.yml Configuration
The project uses the following Docker setup:

YAML
services:
  postgres:
    image: postgres:14-alpine
    container_name: mydotnetpostgres
    restart: unless-stopped

    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: dotnettaskmanager

    ports:
      - "5432:5432"

    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
Start the Database Container
Run Docker Compose from the solution root to start PostgreSQL in detached mode:

PowerShell
docker compose up postgres -d
Connection String Configuration
For local development when running the API outside Docker (dotnet run or Visual Studio), use the following connection string in src/TaskManager.Api/appsettings.Development.json or .NET User Secrets:

JSON
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dotnettaskmanager;Username=postgres;Password=postgres"
  }
}


3. Standard Migration Workflow
Whenever you add, modify, or delete domain entities or update Fluent API mappings in TaskManagerDbContext, follow these steps from the solution root directory.

Step 1: Generate a Migration
Generate the C# migration files. Use a clear, descriptive name in PascalCase describing the schema change:

PowerShell
dotnet ef migrations add <MigrationName> `
  --project src/Infrastructure `
  --startup-project src/TaskManager.Api `
  --output-dir Migrations
Example:

PowerShell
dotnet ef migrations add AddPriorityToTaskItem `
  --project src/Infrastructure `
  --startup-project src/TaskManager.Api `
  --output-dir Migrations

Step 2: Apply the Migration to Your Docker Database
Update your PostgreSQL database running inside Docker with the newly generated migration:

PowerShell
dotnet ef database update `
  --project src/Infrastructure `
  --startup-project src/TaskManager.Api

4. Custom SQL Script Migrations (Data Backfills & Complex Logic)
When a schema change requires moving, copying, or transforming existing table data (for example, combining columns or processing data into a new table), do not write C# application code inside migrations. Instead, execute raw SQL inside the migration's Up() method.

How to Write a Custom SQL Migration
Generate an Empty or Partial Migration:

PowerShell
dotnet ef migrations add BackfillTaskSummary `
  --project src/Infrastructure `
  --startup-project src/TaskManager.Api `
  --output-dir Migrations
Embed Custom SQL via migrationBuilder.Sql(...):
Open the generated file in src/Infrastructure/Migrations/ and add your raw PostgreSQL script directly after the schema modifications:

C#
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Migrations
{
    public partial class BackfillTaskSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Schema change
            migrationBuilder.AddColumn<string>(
                name: "FullSummary",
                table: "TaskItemEntities",
                type: "character varying(500)",
                nullable: true);

            // 2. Custom SQL Data Transformation (PostgreSQL Dialect)
            migrationBuilder.Sql(@"
                UPDATE ""TaskItemEntities"" AS t
                SET ""FullSummary"" = CONCAT(c.""Name"", ': ', t.""Title"")
                FROM ""Categories"" AS c
                WHERE t.""CategoryId"" = c.""Id"";
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullSummary",
                table: "TaskItemEntities");
        }
    }
}

Rules for Custom SQL Migrations:

Use PostgreSQL Dialect: Since our engine is postgres:14-alpine, ensure table/column quotes and functions (e.g., CONCAT, TO_CHAR) use valid PostgreSQL syntax.

Make Columns Nullable First: If adding a required column that needs backfilled data, create it as nullable (nullable: true), execute the migrationBuilder.Sql(...) statement, and then alter it to non-nullable if necessary.

Never Instantiate DbContext inside Migrations: Always use migrationBuilder.Sql(...) to keep migration snapshots immutable over time.

5. Source Control Policy (Git)
All migration files generated inside src/Infrastructure/Migrations/ are part of the core codebase and must be committed to Git.

When submitting a Pull Request, verify that the following files are included:

src/Infrastructure/Migrations/<Timestamp>_<MigrationName>.cs

src/Infrastructure/Migrations/<Timestamp>_<MigrationName>.Designer.cs

src/Infrastructure/Migrations/TaskManagerDbContextModelSnapshot.cs

6. Reverting Local Migrations (Troubleshooting)
If you made a mistake on a local migration that has not yet been pushed or merged:

Revert the local PostgreSQL database state to the previous migration:

PowerShell
dotnet ef database update <PreviousMigrationName> `
  --project src/Infrastructure `
  --startup-project src/TaskManager.Api
Remove the uncommitted migration files:

PowerShell
dotnet ef migrations remove `
  --project src/Infrastructure `
  --startup-project src/TaskManager.Api