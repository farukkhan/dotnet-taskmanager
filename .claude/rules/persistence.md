---
paths:
  - "src/Infrastructure/**"
---

# Persistence how-to

These rules add to the *Database rules* in `CLAUDE.md`, which always apply.
Design background: `docs/architecture.md`. Full migration workflow, including raw-SQL backfills: `docs/README-MIGRATIONS.md`.

## Queries

Be deliberate about query behaviour and mention the choice when it is relevant:
- tracking vs. `AsNoTracking` (reads that don't save use `AsNoTracking`)
- N+1 queries
- indexes for new filter or sort columns
- `ExecuteUpdate`/`ExecuteDelete` vs. loading entities. Bulk operations run SQL directly, so EF Core does not add the `Version` concurrency-token check. Check and increment `Version` yourself, or load the entity instead.

## Migration commands (repository root, PowerShell)

```powershell
dotnet ef migrations add <PascalCaseName> --project src/Infrastructure --startup-project src/TaskManager.Api --output-dir Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/TaskManager.Api
```
