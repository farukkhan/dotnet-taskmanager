---
paths:
  - "src/TaskManager.Api/**"
  - "src/Application/UseCases/**"
  - "src/Application/Exceptions/**"
  - "src/Domain/Exceptions/**"
---

# API architecture and error-handling rules

These rules apply to `src/TaskManager.Api` and to the exceptions it passes on to clients. They add to `CLAUDE.md`. If the two conflict, `CLAUDE.md` wins.

## Layering

- Controllers are thin. An action maps the request DTO to a command or query, calls **one** use-case interface (`I<Name>UseCase.ExecuteAsync`), maps the domain result to a response DTO, and returns an `IActionResult`. Controllers contain no business rules, no invariant checks and no persistence code.
- Inject use-case interfaces through the controller's primary constructor, as `TaskItemController` does. Never inject `TaskManagerDbContext`, repositories or any other Infrastructure type.
- Business rules belong in the domain (`TaskItem`). Loading data and existence checks belong in use cases.
- `Program.cs` only composes the app (`AddApplication()`, `AddInfrastructure(...)`, the pipeline). Register new use cases in `Application/DependencyInjection.cs`, not in `Program.cs`.

## Controllers and routes

- Use attribute-routed controllers with `[ApiController]` and `[Route("api/[controller]")]`. Don't switch to Minimal APIs without approval.
- Resource ids go in the route (`{id}`) and payloads in the body. The route id is the one that counts. Never read the id from the body.
- Action names end in `Async` and return `Task<IActionResult>`.
- Declare every status code an action can return with `[ProducesResponseType]`, including error codes (404, 409, ...).
- Every action takes a `CancellationToken cancellationToken` and passes it to the use case. Don't use `CancellationToken.None` in new code.
- Success responses: GET returns `200`. POST returns `201` through `CreatedAtAction` pointing at the get-by-id action. PUT returns `200` with the updated resource, including its new `Version`. DELETE returns `200 Ok()` today. Keep that unless a Jira issue changes it.

## DTOs and mapping

- Request DTOs go in `Dtos/Request` (`<Action><Resource>Dto`, e.g. `UpdateTaskItemDto`) and response DTOs in `Dtos/Response`. Follow the existing style: classes with `get; set;` properties.
- Never return domain models (`TaskItem`) or persistence entities (`TaskItemEntity`) from an action.
- Validate request DTOs with DataAnnotations (`[Required]`, `[StringLength]`). `[ApiController]` then returns 400 `ValidationProblemDetails` before the use case runs. Take the limits from the domain constants (e.g. `TaskItem.TitleMaxLength`) and never repeat the numbers. DTO validation is the first check; the domain still enforces the rules.
- Map domain objects to response DTOs with extension methods in `Mappers/Response/ResponseMapper.cs` (`To<Name>ResponseDto()`). The controller builds commands and queries inline from request DTOs. No AutoMapper.

## Optimistic concurrency in the API

- Every task response DTO includes `Version`.
- Every request that changes an existing task includes the client's `Version`, and the controller passes it into the command unchanged.
- Never fill in the expected version from the database. If you do, the check compares the stored value with itself and never fails. That was the bug fixed in PREP-7.

## Error handling

Errors are signalled by throwing exceptions from Domain, Application or Infrastructure. The API maps them to HTTP responses **in one central place**.

| Exception | Layer / thrown by | HTTP status |
|---|---|---|
| `NotFoundException` | Application: use cases, repository | 404 Not Found |
| `VersionConflictException` | Domain: `TaskItem` version check | 409 Conflict |
| `ConcurrencyException` | Application: repository (stored version mismatch, `DbUpdateConcurrencyException`) | 409 Conflict |
| `DomainValidationException` | Domain: invariant validation in `TaskItem` | 400 Bad Request |
| anything else, including `ArgumentException` | n/a | 500 with a generic `ProblemDetails` body and no exception details |

- Domain rules throw `DomainValidationException`, **never** `ArgumentException`. Framework code also throws `ArgumentException` for server-side bugs, so it must stay a 500.

- **Don't catch exceptions in controllers** to turn them into status codes. Map them only in the central handler.
- Domain and Application never reference HTTP types or status codes.
- Error bodies use `ProblemDetails` (RFC 9457) in the same shape for every error.
- Infrastructure translates EF Core exceptions (e.g. `DbUpdateConcurrencyException`) into Application exceptions. EF Core types never reach the API.
- Exception messages must be safe to show a client: no SQL, connection details or stack traces.
- Don't return `null`, `bool` or result types to signal errors from use cases. The one existing exception: get-by-id returns `null`, and the controller turns that into `404`.
- When you add a new exception type, add it to this table and to the central mapping, and add a test for it.

**Where the mapping lives:** `ExceptionHandling/ApiExceptionHandler.cs` (an `IExceptionHandler`), registered in `Program.cs` together with `AddProblemDetails()` and `app.UseExceptionHandler()`. Add new mappings there. Exceptions it doesn't recognise fall through to the default handler, which returns a generic 500. The tests are in `tests/TaskManager.Api.Tests/ExceptionHandling`.

## Testing API behaviour

- Test status codes and `ProblemDetails` bodies in `tests/TaskManager.Api.Tests`. These tests run the real HTTP pipeline through `WebApplicationFactory` (`TaskManagerApiFactory`, Production environment) and replace use cases with stubs using `ConfigureTestServices`. They need no database.
- Behaviour that depends on the database, such as a real concurrent update, belongs in `tests/TaskManager.Api.IntegrationTests` against real PostgreSQL. See *Testing* in `CLAUDE.md`.
- Test mapping and HTTP logic at the API level. Test domain rules in `Domain.Tests`, not through HTTP.
