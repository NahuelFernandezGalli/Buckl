# ADR-0024: Apply migrations explicitly with the owner role

- **Status:** Accepted
- **Date:** 2026-09-22
- **Deciders:** NahuelFernandezGalli

## Context

Row-Level Security only protects data if the role the API uses at runtime cannot bypass it: that
role must not own the tables, must not be a superuser and must not have `BYPASSRLS`. Schema
changes, on the other hand, need the owner. EF Core offers to migrate at startup, which would force
the API to run as the owner.

## Decision

Migrations live in `Buckl.Infrastructure/Persistence/Migrations` and are applied only with the
owner role, explicitly, with `dotnet ef database update` (the tool is pinned as a local tool in
`apps/api/dotnet-tools.json`). The owner's connection string comes from the
`BUCKL_MIGRATIONS_CONNECTION` environment variable and is read by a design-time context factory,
so migrating never starts the API. The API never calls `Migrate()` and connects only as
`buckl_app`.

`buckl_app` is created outside migrations, because its password is a secret: from the Neon SQL
console for real databases and by the test fixture for containers. Migrations grant it privileges
and fail with a clear message if it does not exist. Phase 9 applies the same migrations in the
deploy pipeline, still as a separate step with the owner role.

## Alternatives considered

- **Migrate at startup** — convenient, but the API would need the owner role and could bypass
  Row-Level Security; a failed migration would also take the API down.
- **Hand-written SQL scripts** — full control, but the EF model and the schema would drift with
  nothing to detect it; `HasPendingModelChanges` covers that today.
- **Create the application role inside a migration** — the password would have to live in the
  repository or be injected into SQL at migration time.

## Consequences

### Positive

- The runtime role cannot alter the schema or bypass Row-Level Security.
- The integration tests migrate a real Postgres the same way production will.

### Negative

- One more manual step on a new database: create `buckl_app` before the first migration.
- Two connection strings per environment, one of them only for migrations.
