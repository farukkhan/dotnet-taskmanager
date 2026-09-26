# Architecture

This page describes the architecture as it is committed on `master`, for AI coding agents and developers.
The rules for changing the code are in `CLAUDE.md` and `.claude/rules/`. If this page and those rules disagree, the rules win.
Items marked **Needs verification** are unclear or look unintended. Check them before relying on them.

## Projects and layers

All projects target `net10.0`. The solution is `dotnet-taskmanager.slnx`.

| Project | Role | Key types |
|---|---|---|
| `src/Domain` | Domain model and business rules. No NuGet packages. | `Models/TaskItem`, `Exceptions/VersionConflictException`, `Exceptions/DomainValidationException` |
| `src/Application` | Use cases, commands and queries, ports, application exceptions | `UseCases/*UseCase`, `Ports/ITaskItemRepository`, `Exceptions/NotFoundException`, `Exceptions/ConcurrencyException` |
| `src/Infrastructure` | EF Core persistence (Npgsql / PostgreSQL), repository adapter, migrations | `Persistence/TaskManagerDbContext`, `Persistence/Models/TaskItemEntity`, `Persistence/Mappers/TaskItemMapper`, `Persistence/Adapters/TaskItemRepository` |
| `src/TaskManager.Api` | ASP.NET Core Web API (controllers), DTOs, API mappers, exception handling, composition root | `Controllers/TaskItemController`, `ExceptionHandling/ApiExceptionHandler`, `Program.cs` |
| `src/TaskManager.Web/TaskManager.Web` | Blazor Web App host (experimental) | `Program.cs`, `Components/*` |
| `src/TaskManager.Web/TaskManager.Web.Client` | Blazor client project: pages, API client, client-side model | `Services/TaskApiClient`, `Models/TaskItem`, `Pages/*` |

Test projects that exist today are `tests/Domain.Tests` and `tests/TaskManager.Api.Tests`. `Application.Tests` and `TaskManager.Api.IntegrationTests` are planned but don't exist yet.

## Dependency direction

```
TaskManager.Api ──► Application ──► Domain
      │                 ▲             ▲
      └──► Infrastructure ────────────┘
           (implements Application ports)

TaskManager.Web ──► TaskManager.Web.Client      (no backend references; HTTP only)
```

- These are the project references in the `.csproj` files:
  - Application references Domain.
  - Infrastructure references Application and Domain.
  - Api references Application and Infrastructure.
- **Ports and adapters.** Application defines `ITaskItemRepository`, and Infrastructure implements it as `TaskItemRepository`.
- **What stays inside Infrastructure.** `TaskManagerDbContext`, `TaskItemEntity`, `TaskItemMapper` and `TaskItemRepository` are all `internal`, so EF Core types can't leak out of Infrastructure.
- **Composition.** `Program.cs` calls `AddInfrastructure(configuration)` and `AddApplication()`. Each layer registers its own services in its `DependencyInjection.cs`.
  - Use cases are registered as scoped.
  - The repository is registered as scoped.
  - The `DbContext` is registered with `AddDbContext` and `UseNpgsql`.

## Request flow

```
HTTP request
  → TaskItemController            (maps request DTO → command/query)
  → I<Name>UseCase.ExecuteAsync   (loads data, existence checks, calls domain methods)
  → TaskItem (domain)             (enforces invariants, version check)
  → ITaskItemRepository           (port)
  → TaskItemRepository            (adapter: TaskItemMapper, EF Core, TaskManagerDbContext)
  → PostgreSQL
  ← domain TaskItem ← use case ← controller maps it to a response DTO (Mappers/Response/ResponseMapper.cs)
```

Exceptions thrown anywhere in this chain go to `ApiExceptionHandler` (see *Error handling*).

The endpoints are all on `api/TaskItem`:

| Method | Route | Use case | Success |
|---|---|---|---|
| GET | `/` | `GetTaskItemsUseCase` | 200 |
| GET | `/{id}` | `GetTaskItemUseCase` (returns `null` when missing) | 200 / 404 |
| POST | `/` | `CreateTaskItemUseCase` | 201 (`CreatedAtAction`) |
| PUT | `/{id}` | `UpdateTaskItemUseCase` | 200 |
| DELETE | `/{id}` | `DeleteTaskItemUseCase` | 200 |

`CompleteTaskItemUseCase` and `ReopenTaskItemUseCase` and their commands exist, but `CompleteTaskItemUseCase` throws `NotImplementedException`. Neither use case is registered in DI, and no endpoint calls them.

## The TaskItem aggregate

`Domain/Models/TaskItem` is the only aggregate.

- **Fields:** `Id`, `Title`, `Description?`, `IsCompleted`, `CreatedAt`, `UpdatedAt?`, `Version`. All setters are private.
- **Creation:** `new TaskItem(title, description)` sets `CreatedAt = DateTime.UtcNow` and `Version = 1`.
- **Rehydration:** `TaskItem.Load(...)` rebuilds a task from stored values. It still validates title and description.
- **Invariants:** they are checked inside the method that assigns the field.
  - `Title` must be 1 to `TaskItem.TitleMaxLength` (200) characters.
  - `Description` must be `null` or at most `TaskItem.DescriptionMaxLength` (2000) characters.
  - A violation throws `DomainValidationException`.
  - The API request DTOs reuse these constants in their DataAnnotations (see *Error handling*).
- **State changes:**
  - `Update(title, description, expectedVersion)` checks the version first, then updates title and description through private `UpdateTitle` and `UpdateDescription`.
  - `Complete()` and `Reopen()` change `IsCompleted`. They don't check the version.
  - Each method only sets `UpdatedAt = DateTime.UtcNow` if a value actually changed.
- **The domain never increments `Version`.** Persistence does that (see below).

## Separation of domain and persistence models

- `TaskItem` (Domain) and `TaskItemEntity` (Infrastructure) are separate classes. `TaskItemEntity` is a plain class with public setters and no behaviour.
- `TaskItemMapper` holds two extension methods:
  - `ToDomain()` goes through `TaskItem.Load`.
  - `ToEntity()` copies all fields.
- **Why:** the domain model stays free of EF Core attributes, public setters and parameterless constructors. The cost is an explicit mapping step and a second read on update.

## Persistence with EF Core

- **Provider:** `Npgsql.EntityFrameworkCore.PostgreSQL` on PostgreSQL 14, which runs from `docker-compose.yml`.
- **Configuration:** the model is configured with the Fluent API in `TaskManagerDbContext.OnModelCreating`. There are no data annotations.
  - `Title` is required with a maximum length of 200.
  - `Description` is optional with a maximum length of 2000.
  - `Version` is required and marked as a concurrency token (`IsConcurrencyToken()`).
- **Schema changes** go through EF Core migrations in `src/Infrastructure/Migrations`. The only migration so far is `InitialCreate`. The workflow is in `docs/README-MIGRATIONS.md`.

How `TaskItemRepository` handles each operation:

| Method | Query behaviour |
|---|---|
| `CreateAsync` | Builds a new entity, then `AddAsync` and `SaveChangesAsync`. The database generates the `Id`. |
| `GetAllAsync` | Projects entities to domain objects in `Select` and applies `AsNoTracking`. There is no filtering or paging. |
| `GetByIdAsync` | `AsNoTracking` with `FirstOrDefaultAsync`. Returns `null` if the task isn't found. |
| `UpdateAsync` | Loads a tracked entity. Throws `NotFoundException` if it's missing and `ConcurrencyException` on a version mismatch. Copies `Title` and `Description`, runs `Version++`, then `SaveChangesAsync`. |
| `DeleteByIdAsync` | Uses `ExecuteDeleteAsync`, which runs a single SQL `DELETE` without loading the entity. Zero rows affected throws `NotFoundException`. |

**Needs verification:** `UpdateAsync` doesn't copy `IsCompleted` or `UpdatedAt` to the entity. Changes to those fields aren't persisted through this path.

**Needs verification (configuration):** `appsettings.json` contains a hardcoded `ConnectionStrings:DefaultConnection`. `Program.cs` calls `Env.Load()` (DotNetEnv) in Development, and `.env` defines `POSTGRES_USER`, `POSTGRES_PASSWORD` and `POSTGRES_DB` for Docker Compose. It isn't clear whether the `.env` values override the connection string.

## Optimistic concurrency

The design uses an application-managed integer `Version` column. It doesn't use PostgreSQL `xmin`, and it doesn't lock rows.

1. The client reads a task, and the response DTO includes `Version`.
2. The client sends `Version` back in `UpdateTaskItemDto`. The controller passes it into `UpdateTaskItemCommand` unchanged.
3. The use case loads the task (untracked) and calls `TaskItem.Update(..., expectedVersion)`. A mismatch throws `VersionConflictException` (Domain).
4. The repository loads the entity again, this time tracked, and compares versions. A mismatch throws `ConcurrencyException` (Application).
5. The repository runs `Version++` and saves. Because `Version` is a concurrency token, EF Core adds `WHERE "Version" = <original>` to the `UPDATE`. If another write happened between the read and the save, EF Core throws `DbUpdateConcurrencyException`, and the repository translates it to `ConcurrencyException`.

All three checks end in HTTP 409. The expected version must always come from the client, never from the database: that bug was fixed in PREP-7.

## Exceptions and error handling

- Layers signal errors by throwing exceptions. Mapping them to HTTP happens in one place, `TaskManager.Api/ExceptionHandling/ApiExceptionHandler.cs`.
  - It is an `IExceptionHandler`, registered in `Program.cs` with `AddProblemDetails()` and `app.UseExceptionHandler()`.
  - Responses are `ProblemDetails` bodies with `Status` and `Detail = exception.Message`.

| Exception | Thrown by | HTTP |
|---|---|---|
| `NotFoundException` | Use cases, repository | 404 |
| `VersionConflictException` | `TaskItem.Update` | 409 |
| `ConcurrencyException` | Repository (version mismatch, `DbUpdateConcurrencyException`) | 409 |
| `DomainValidationException` | `TaskItem` invariant validation | 400 |
| Anything else, including `ArgumentException` | n/a | 500 from the default handler |

- **Validation happens twice.**
  - First, the request DTOs (`CreateTaskItemDto`, `UpdateTaskItemDto`) use DataAnnotations (`[Required]`, `[StringLength(TaskItem.TitleMaxLength)]`, ...). `[ApiController]` returns 400 `ValidationProblemDetails` before the use case runs.
  - Second, the domain enforces the same rules through `DomainValidationException`.
  - Domain rules never throw `ArgumentException`, because framework code throws it for server-side bugs and it must stay a 500.
- The one path that doesn't use an exception is get-by-id. The use case returns `null`, and the controller returns `Problem(..., 404)`.
- Domain and Application don't reference HTTP types. Infrastructure translates EF Core exceptions before they leave the layer.
- Detailed rules are in `.claude/rules/api.md`.

## Blazor front end (experimental)

- **Hosting:** `TaskManager.Web` is a Blazor Web App host. It enables both Interactive Server and Interactive WebAssembly render modes, and loads components from `TaskManager.Web.Client`.
- **Backend access:** the client project talks to the API only over HTTP, through `Services/TaskApiClient` and the `api/taskitem` routes. It has its own `Models/TaskItem` DTO, including `Version`, and references no backend project.
- **API address:** the base URL `http://localhost:5216/` is hardcoded in both `Program.cs` files. It isn't read from configuration.
- **Pages:** `Pages/Tasks.razor` (`/tasks`) and `Pages/TaskEdit.razor` (`/tasks/create`, `/tasks/edit/{Id}`) both use `@rendermode InteractiveServer`.
  - They run on the server and use the `TaskApiClient` that the host registers with `AddHttpClient`.
  - As a result, the browser currently makes no direct calls to the API.

**Needs verification:**
- In `TaskManager.Web.Client/Program.cs`, the `HttpClient` and `TaskApiClient` registrations come after `await builder.Build().RunAsync()`, so they never take effect. Components rendered with WebAssembly would have no `TaskApiClient`.
- The API has no CORS configuration. Calls straight from the browser (WebAssembly render mode) would need it.
