# CLAUDE.md

Guidance for AI coding agents working in this repository.

## Purpose of this project

This is a **learning project**, not a production system. The human developer uses it to practise:
- .NET 10 / ASP.NET Core
- Clean Architecture
- Entity Framework Core and SQL (PostgreSQL)
- Automated testing
- AI-assisted and agentic software development
- Interview preparation

Understanding comes before speed. If a change teaches something (a pattern, a tradeoff, an EF Core behaviour), explain it briefly. **The human developer makes the final decisions.** You propose, explain and implement what they approve.

## Solution layout

Solution: `dotnet-taskmanager.slnx` (all projects target `net10.0`)

| Project | Role | May reference |
|---|---|---|
| `src/Domain` | Domain models and business rules (`TaskItem`) | nothing |
| `src/Application` | Use cases, commands/queries, ports (`ITaskItemRepository`), application exceptions | Domain |
| `src/Infrastructure` | EF Core `TaskManagerDbContext`, repository adapters, persistence entities and mappers, migrations | Application, Domain |
| `src/TaskManager.Api` | ASP.NET Core Web API: controllers, request/response DTOs, DI composition | Application, Infrastructure |
| `src/TaskManager.Web/TaskManager.Web` | Blazor Web App host (experimental) | Web.Client |
| `src/TaskManager.Web/TaskManager.Web.Client` | Blazor WASM client; talks to the API over HTTP only | nothing in the backend |
| `tests/*` | xUnit test projects (see *Testing*) | the project under test |

Add new projects to `dotnet-taskmanager.slnx` as flat `<Project Path="..." Id="..." />` entries, like the existing ones. Don't add `<Folder>` elements.

### Dependency rules (do not break)
- Domain has no dependencies: no EF Core, no ASP.NET, no NuGet packages unless approved.
- Application defines **ports** (interfaces) and Infrastructure implements them as **adapters**.
- EF Core types stay in Infrastructure. The persistence model `TaskItemEntity` is kept separate from the domain model `TaskItem` and mapped by `TaskItemMapper`. Do not merge them.
- The API never talks to `DbContext` directly. It goes through use-case interfaces.
- The Blazor front end never references backend projects.

## Existing conventions (follow them)

- **One use case per operation.** Each has an interface in `Application/UseCases/Interfaces/I<Name>UseCase.cs` and an implementation in `Application/UseCases/<Name>UseCase.cs` exposing `ExecuteAsync(<Command|Query>, CancellationToken)`.
- **Commands and queries** are `record` types in `Application/Commands` and `Application/Queries`.
- **DI registration** goes in each layer's `DependencyInjection.cs` (`AddApplication()`, `AddInfrastructure(...)`). Register every new use case there.
- **Domain invariants** live in the domain model. State changes go through methods (`UpdateTitle`, `Complete`, `Reopen`), setters are private, and rehydration uses `TaskItem.Load(...)`.
- **Optimistic concurrency** uses an integer `Version` on the task.
- **API mapping** uses extension methods in `TaskManager.Api/Mappers`. Request and response DTOs live in `Dtos/Request` and `Dtos/Response`.
- File-scoped namespaces, nullable reference types, `async`/`await` with `CancellationToken` passed through.
- Match the style of the file you are editing. Primary constructors and classic constructors both exist in the codebase. Don't convert one to the other unless asked.

## Workflow

1. **Inspect first.** Read the relevant existing code before proposing or making changes. Don't assume a file, method or pattern exists; check.
2. **Plan non-trivial changes.** For anything beyond a small, local fix, give a plan first and wait for approval (see *Jira workflow* for what the plan contains).
3. **Keep changes small and reviewable.** Do one concern per change. Don't reformat, rename or "tidy up" unrelated code. The 10-file PR limit applies (see *Git and pull request workflow*).
4. **Don't over-engineer.** Don't add abstractions, layers, generic base classes, MediatR, AutoMapper, result types, specification patterns and so on unless there is a concrete need and the developer approves. Build the simplest thing that fits the existing architecture.
5. **Explain tradeoffs.** When there are several reasonable approaches, list them briefly with their pros and cons and recommend one. Don't choose silently.
6. **Get explicit approval for architectural changes.** That includes new projects, new layers, changed dependency directions, new frameworks or major packages, changes to the persistence approach, and restructured folders.
7. **Tests.** Add or update tests for every behavioural change, then run them (see *Testing*).
8. **Report honestly.** Say what you changed, what you verified, and what you did not verify.

## Jira workflow

- **Project:** Jira project key `PREP` on `farukinfo.atlassian.net`. Issue types: Story, Task, Bug, Subtask.
- **Known gaps and bugs are tracked as Jira issues, not in this file.** When you find one outside the current scope, report it and propose a Jira issue for it. Don't fix it silently.
- **The Jira issue and its acceptance criteria are the main requirements.** If they are missing, unclear or contradict the codebase, ask before implementing. Don't guess.
- **Before implementing:** inspect the relevant code, then give an implementation plan that includes:
  - the Jira key and a one-line summary
  - the approach and the files to touch, with an estimated changed-file count
  - how each acceptance criterion will be met and tested
  - open questions, risks and alternatives worth considering
- **Wait for human approval** of the plan before implementing any non-trivial change.
- **After implementing:** go through each acceptance criterion and mark it ✅ met (with how it was verified: test name, manual check), ⚠️ partially met, or ❌ not met.
- **Report every requirement you could not meet,** and say why. Never quietly narrow the scope.

## Git and pull request workflow

- **One branch per Jira issue.** Never work directly on `master`. Branch name: `feature/<JIRA-KEY>-<short-description>`, e.g. `feature/PREP-8-enable-agentic-coding`.
- **Keep each PR focused** on one Jira issue or one coherent change. Don't bundle unrelated fixes; mention them separately.
- **A PR changes at most 10 files.** Tests, DTOs, DI registrations and all three files of an EF migration all count. If the work would go over 10, stop and propose a split before implementing.
- **Before creating a PR:**
  1. Run `dotnet build dotnet-taskmanager.slnx`. It must succeed.
  2. Run the relevant tests and report the actual results.
  3. Review the full diff (`git diff master...HEAD`) for unrelated changes, debug code, commented-out code and secrets.
- **Get explicit human approval** before each commit, push, PR creation or merge. Approval for one step does not cover the next.
- **The human developer** does the final PR review and decides whether to merge it.
- Commit messages and PR titles start with the Jira key, e.g. `<JIRA-KEY>: Add complete/reopen endpoints`. Link the Jira issue in the PR description.

## Testing

Tests use **xUnit** and live under `tests/`. The layout is `tests/Domain.Tests`, `tests/Application.Tests`, `tests/TaskManager.Api.Tests` and `tests/TaskManager.Api.IntegrationTests`. Check which of them already exist before adding tests.

- Before creating a new test project, confirm it with the developer and add it to the solution as described in *Solution layout*.
- Domain tests: pure unit tests of invariants and state transitions.
- Application tests: use cases against a fake or mocked `ITaskItemRepository`.
- API tests: the HTTP pipeline (routing, status codes, `ProblemDetails`) through `WebApplicationFactory`, with use cases stubbed. No database.
- Integration tests: API and EF Core against real PostgreSQL. Discuss the options (e.g. Testcontainers vs. the Docker Compose DB) before choosing. Prefer not to use the EF InMemory provider for behaviour that depends on the database.
- Name tests `MethodOrScenario_Condition_ExpectedResult`.
- After implementing, run the relevant tests and report the real results, including failures.

## Commands (run from the repository root, PowerShell)

```powershell
# Build and test
dotnet build dotnet-taskmanager.slnx
dotnet test  dotnet-taskmanager.slnx

# Local database (needs .env copied from .env.example)
docker compose up postgres -d

# Run the API
dotnet run --project src/TaskManager.Api

# EF Core migrations
dotnet ef migrations add <PascalCaseName> --project src/Infrastructure --startup-project src/TaskManager.Api --output-dir Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/TaskManager.Api
```

For the full migration workflow, including data backfills written in raw SQL, see `docs/README-MIGRATIONS.md`.

## Database and SQL rules

- The database is PostgreSQL 14. Any raw SQL must use PostgreSQL syntax (quoted identifiers, etc.).
- Schema changes go through EF Core migrations. Commit all three files: the migration, its `.Designer.cs`, and the model snapshot.
- Never edit a migration that has already been applied or committed. Add a new one instead.
- For data transformations inside migrations, use `migrationBuilder.Sql(...)`. Never use `DbContext`.
- Be deliberate about query behaviour. Mention tracking vs. `AsNoTracking`, N+1 problems, indexes, and `ExecuteUpdate`/`ExecuteDelete` vs. loading entities when they are relevant.
- Don't run `database update`, drop databases or delete Docker volumes without asking.

## Secrets and configuration

- `.env` is git-ignored and holds local credentials. Never commit it, print it or copy its values into other files.
- When you add a configuration key, add it with a safe placeholder value to `.env.example`.

## Interview-preparation mode

When relevant, briefly contrast the modern approach with common legacy or alternative approaches an interviewer might ask about. Examples:
- Minimal hosting (`Program.cs`) vs. `Startup.cs`; controllers vs. Minimal APIs
- EF Core vs. EF6, Dapper or raw ADO.NET
- `async`/`await` vs. synchronous or legacy async patterns
- Built-in DI vs. third-party containers or service locator
- Records and primary constructors vs. classic classes
- Optimistic concurrency with a version column vs. `rowversion`/`xmin` vs. pessimistic locking
- .NET 10 vs. .NET Framework (e.g. `web.config`, `Global.asax`, Web API 2)

Keep this short, a few lines, unless the developer asks for more. Don't implement legacy approaches in the codebase unless asked.
