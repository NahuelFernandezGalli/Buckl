# Buckl API

The .NET 10 solution behind Buckl. How it is organized: [API architecture](../../docs/architecture/api.md).

## Prerequisites

- .NET SDK 10.0.401 or a later 10.0 feature band (pinned by `global.json`).
- Docker Desktop, running: the integration tests start Postgres 17 in a container.
- Local tools, once per clone: `dotnet tool restore` (installs `dotnet-ef` 10).

Run every command from this directory. The SDK reads `global.json` from the current directory, and
the test runner needs it.

## Tests

```bash
dotnet test
```

One class: `dotnet test --filter-class Buckl.Infrastructure.Tests.Persistence.InitialSchemaTests`.

## Running the API locally

The API runs against the `dev` branch of the Neon project, as `buckl_app`. One-time setup: create
the branch and the role, and migrate the branch, as described in "Database roles" and "Database
migrations" below.

Store the application role's connection string outside the repository, once:

```bash
dotnet user-secrets set "ConnectionStrings:Buckl" "Host=<ep-...>.neon.tech;Database=buckl;Username=buckl_app;Password=<...>;SSL Mode=Require" --project src/Buckl.Api
```

Then:

```bash
dotnet run --project src/Buckl.Api
```

The API listens on `http://localhost:5080`. Until phase 5 it runs only in the Development
environment and identifies callers by the `X-Dev-User` header: any value is a user, so
`dev|alice` and `dev|bob` are two users. `src/Buckl.Api/Buckl.Api.http` walks through the whole
flow for both, and the API reference is at `http://localhost:5080/scalar/v1`.

## Database migrations

Migrations live in `src/Buckl.Infrastructure/Persistence/Migrations`. The API never applies them;
the owner role does, explicitly ([ADR-0024](../../docs/adr/0024-apply-migrations-explicitly-with-the-owner-role.md)).

Add a migration after changing the model (no database needed):

```bash
dotnet tool run dotnet-ef migrations add <Name> --project src/Buckl.Infrastructure --startup-project src/Buckl.Infrastructure --output-dir Persistence/Migrations
```

Apply migrations with the owner's connection string in `BUCKL_MIGRATIONS_CONNECTION`. The
application role `buckl_app` must exist first (see "Database roles").

```powershell
$env:BUCKL_MIGRATIONS_CONNECTION = "Host=...;Database=buckl;Username=<owner>;Password=...;SSL Mode=Require"
dotnet tool run dotnet-ef database update --project src/Buckl.Infrastructure --startup-project src/Buckl.Infrastructure
Remove-Item Env:BUCKL_MIGRATIONS_CONNECTION
```

## Database roles

| Role                  | Used by            | Privileges                                              |
| --------------------- | ------------------ | ------------------------------------------------------- |
| owner (Neon: default) | migrations only    | owns the schema                                         |
| `buckl_app`           | the API at runtime | granted by the migrations; no `BYPASSRLS`, owns nothing |

Create `buckl_app` once per database, from a SQL console connected as the owner, with a generated
password that never goes into the repository:

```sql
create role buckl_app login password '<generated password>' nobypassrls;
```
