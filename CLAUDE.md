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

Solution: `dotnet-taskmanager.slnx` (all projects target `net10.0`):
`src/Domain`, `src/Application`, `src/Infrastructure`, `src/TaskManager.Api`,
`src/TaskManager.Web/TaskManager.Web` (Blazor host, experimental), `src/TaskManager.Web/TaskManager.Web.Client`.
Test projects are under `tests/` (see *Testing*).

Add new projects to `dotnet-taskmanager.slnx` as flat `<Project Path="..." Id="..." />` entries, like the existing ones. Don't add `<Folder>` elements.

## Architecture reference

`docs/architecture.md` describes the layers, request flow, the `TaskItem` aggregate, persistence, optimistic concurrency, error handling and the Blazor front end. **Read it before you:**
- add or change a use case, port, repository method, entity or migration
- touch concurrency (`Version`) or the exception-to-HTTP mapping
- change anything in `TaskManager.Web*`
- propose a structural or cross-layer change

If the code contradicts the doc, trust the code and report the mismatch.

### Dependency rules (do not break)
- Domain → nothing. Application → Domain. Infrastructure → Application, Domain. Api → Application, Infrastructure. Web → Web.Client.
- Domain has no NuGet packages (no EF Core, no ASP.NET) unless approved.
- Application defines **ports** (interfaces) and Infrastructure implements them as **adapters**.
- EF Core types stay in Infrastructure. The persistence model `TaskItemEntity` stays separate from the domain model `TaskItem` and is mapped by `TaskItemMapper`. Do not merge them.
- The API never talks to `DbContext` directly. It goes through use-case interfaces.
- The Blazor front end never references backend projects. It talks to the API over HTTP only.

## Existing conventions (follow them)

- **One use case per operation.** Each has an interface in `Application/UseCases/Interfaces/I<Name>UseCase.cs` and an implementation in `Application/UseCases/<Name>UseCase.cs` exposing `ExecuteAsync(<Command|Query>, CancellationToken)`.
- **Commands and queries** are `record` types in `Application/Commands` and `Application/Queries`.
- **DI registration** goes in each layer's `DependencyInjection.cs` (`AddApplication()`, `AddInfrastructure(...)`). Register every new use case there.
- **Domain invariants** live in the domain model. State changes go through methods (`Update(title, description, expectedVersion)`, `Complete`, `Reopen`), setters are private, and rehydration uses `TaskItem.Load(...)`.
- **Optimistic concurrency** uses an integer `Version` on the task (flow: `docs/architecture.md`).
- File-scoped namespaces, nullable reference types, `async`/`await` with `CancellationToken` passed through.
- Match the style of the file you are editing. Primary constructors and classic constructors both exist in the codebase. Don't convert one to the other unless asked.

## Workflow

1. **Inspect first.** Read the relevant existing code before proposing or making changes. Don't assume a file, method or pattern exists; check.
2. **Plan non-trivial changes, then wait for approval.** Before planning, read the files you will touch and every `.claude/rules/*.md` whose `paths:` match them. The plan contains:
   - the Jira key and a one-line summary
   - the approach and the files to touch, with an estimated changed-file count
   - the rule files applied (e.g. `api.md`, `persistence.md`)
   - how each acceptance criterion will be met and tested
   - open questions, risks and alternatives worth considering
3. **Keep changes small and reviewable.** Do one concern per change. Don't reformat, rename or "tidy up" unrelated code. The 10-file PR limit applies (see *Git and pull request workflow*).
4. **Don't over-engineer.** Don't add abstractions, layers, generic base classes, MediatR, AutoMapper, result types, specification patterns and so on unless there is a concrete need and the developer approves.
5. **Explain tradeoffs.** When there are several reasonable approaches, list them briefly with their pros and cons and recommend one. Don't choose silently.
6. **Get explicit approval for architectural changes:** new projects, new layers, changed dependency directions, new frameworks or major packages, changes to the persistence approach, restructured folders.
7. **Tests.** Add or update tests for every behavioural change, then run them (see *Testing*).
8. **Report honestly.** Mark each acceptance criterion ✅ met (with how it was verified: test name, manual check), ⚠️ partially met, or ❌ not met. Say what you did not verify and every requirement you could not meet, and why. Never quietly narrow the scope.

## Jira

- **Project:** Jira project key `PREP` on `farukinfo.atlassian.net`. Issue types: Story, Task, Bug, Subtask.
- **The Jira issue and its acceptance criteria are the main requirements.** If they are missing, unclear or contradict the codebase, ask before implementing. Don't guess.
- **Known gaps and bugs are tracked as Jira issues, not in this file.** When you find one outside the current scope, report it and propose a Jira issue for it. Don't fix it silently.

## Git and pull request workflow

- **One branch per Jira issue.** Never work directly on `master`. Branch name: `feature/<JIRA-KEY>-<short-description>`, e.g. `feature/PREP-8-enable-agentic-coding`.
- **Keep each PR focused** on one Jira issue or one coherent change. Don't bundle unrelated fixes; mention them separately.
- **A PR changes at most 10 files.** Tests, DTOs, DI registrations and all three files of an EF migration all count. If the work would go over 10, stop and propose a split before implementing. Stacked PRs follow `.claude/rules/git-pr-workflow.md`.
- **Before creating a PR:**
  1. Run `dotnet build dotnet-taskmanager.slnx`. It must succeed.
  2. Run the relevant tests and report the actual results.
  3. Review the full diff (`git diff master...HEAD`) for unrelated changes, debug code, commented-out code and secrets.
- **Get explicit human approval** before each commit, push, PR creation or merge. Approval for one step does not cover the next.
- **The human developer** does the final PR review and decides whether to merge it.
- Commit messages and PR titles start with the Jira key, e.g. `<JIRA-KEY>: Add complete/reopen endpoints`. Link the Jira issue in the PR description.

## Testing

Tests use **xUnit** and live under `tests/`. Existing projects: `tests/Domain.Tests`, `tests/TaskManager.Api.Tests`. Planned: `tests/Application.Tests`, `tests/TaskManager.Api.IntegrationTests`.

- Before creating a new test project, confirm it with the developer and add it to the solution as described in *Solution layout*.
- Domain tests: pure unit tests of invariants and state transitions.
- Application tests: use cases against a fake or mocked `ITaskItemRepository`.
- API tests: HTTP pipeline only, use cases stubbed, no database (details in `.claude/rules/api.md`).
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
```

## Database rules

- PostgreSQL 14. Raw SQL uses PostgreSQL syntax (quoted identifiers, etc.).
- Schema changes only through EF Core migrations. Commit all three files: the migration, its `.Designer.cs`, and the model snapshot.
- Data transformations in migrations use `migrationBuilder.Sql(...)`, never `DbContext`.
- Never edit a migration that has already been applied or committed. Add a new one instead.
- Don't run `database update`, drop databases or delete Docker volumes without asking.
- Query and migration how-to: `.claude/rules/persistence.md` (loads for `src/Infrastructure`) and `docs/README-MIGRATIONS.md`.

## Secrets and configuration

- `.env` is git-ignored and holds local credentials. Never commit it, print it or copy its values into other files.
- When you add a configuration key, add it with a safe placeholder value to `.env.example`.

## Interview-preparation mode

When relevant, add 2–3 lines contrasting the modern approach with a legacy or alternative one an interviewer might ask about (e.g. `Startup.cs`, Minimal APIs, EF6/Dapper, `rowversion`/`xmin`/pessimistic locking, .NET Framework). Don't implement legacy approaches unless asked.
