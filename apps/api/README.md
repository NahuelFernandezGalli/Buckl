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

The API runs against the `dev` branch of the Neon project, as `buckl_app`, and accepts access
tokens from the Auth0 development tenant. One-time setup: create the branch and the role, and
migrate the branch, as described in "Database roles" and "Database migrations" below. Every
setting is listed in [Configuration](../../docs/configuration.md).

Store the local settings outside the repository, once. `Auth0:Domain` is the tenant domain
(without `https://`) and `Auth0:Audience` the identifier of the "Buckl API" registered in it:

```bash
dotnet user-secrets set "ConnectionStrings:Buckl" "Host=<ep-...>.neon.tech;Database=buckl;Username=buckl_app;Password=<...>;SSL Mode=Require" --project src/Buckl.Api
```

```bash
dotnet user-secrets set "Auth0:Domain" "<tenant>.us.auth0.com" --project src/Buckl.Api
```

```bash
dotnet user-secrets set "Auth0:Audience" "https://api.buckl.app" --project src/Buckl.Api
```

Then:

```bash
dotnet run --project src/Buckl.Api
```

The API listens on `http://localhost:5080`. It refuses to start if the Auth0 settings are missing
or malformed, and answers `401` to any request without a valid access token except `GET /health`.
In Development it accepts browser calls from the web app at `http://localhost:5173` and
`http://localhost:4173` (`appsettings.Development.json`).

`src/Buckl.Api/Buckl.Api.http` walks through the whole flow for two users. It reads their access
tokens from `src/Buckl.Api/.env`, which git ignores:

```dotenv
ALICE_TOKEN=<access token of the first test user>
BOB_TOKEN=<access token of the second test user>
```

To get a token, sign in to the web app at `http://localhost:5173` as that user, open the browser's
developer tools, and copy `access_token` from the response of the `oauth/token` request. Tokens
expire after an hour. Never paste a token into an online decoder. The API reference is at
`http://localhost:5080/scalar/v1`.

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
