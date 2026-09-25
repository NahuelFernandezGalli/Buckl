# API

How `apps/api` is organized and how a request travels through it. The stack is .NET 10, ASP.NET
Core controllers, EF Core on Postgres and xUnit v3; testing follows
[the testing strategy](../testing.md). The layering decision is
[ADR-0022](../adr/0022-layer-the-api-with-controllers-and-use-case-handlers.md).

## Projects

| Project                    | Depends on                  | Responsibility                                                      |
| -------------------------- | --------------------------- | ------------------------------------------------------------------- |
| `src/Buckl.Domain`         | nothing                     | Aggregates, value objects, rules and repository ports (phase 2)     |
| `src/Buckl.Application`    | Domain                      | One handler per use case and the ports they need                    |
| `src/Buckl.Infrastructure` | Application                 | EF Core persistence, migrations, Row-Level Security session binding |
| `src/Buckl.Api`            | Application, Infrastructure | Controllers, authentication, error mapping; composition root        |
| `tests/Buckl.*.Tests`      | the layer under test        | One test project per layer, plus `Buckl.Architecture.Tests`         |

`Buckl.Architecture.Tests` fails the build if a layer references one it must not, or if the domain
or the application reference EF Core, Npgsql or ASP.NET Core.

## Composition root

`Program.cs` is the only place that knows every layer. It calls `AddApplication()` and
`AddInfrastructure()`, each defined in its own project, and maps the endpoints.

## Use cases

Each use case is one `sealed` handler class in `Buckl.Application`, grouped by aggregate
(`Garments/`, `Products/`), with a single `HandleAsync` method. Handlers depend on domain ports and
on application ports in `Abstractions/` (`ICurrentUser`, `IUnitOfWork`, `IUserTransactionFactory`,
`IUserProvisioning`), never on EF Core or ASP.NET Core.

| Handler                 | Input                            | Output   | Errors                                                                           |
| ----------------------- | -------------------------------- | -------- | -------------------------------------------------------------------------------- |
| `ListWardrobeHandler`   | `WardrobeFilter`                 | garments | —                                                                                |
| `GetGarmentHandler`     | `GarmentId`                      | garment  | `garment.not_found`                                                              |
| `GetProductHandler`     | `ProductId`                      | product  | `product.not_found`                                                              |
| `CreateGarmentHandler`  | `CreateGarmentCommand`           | garment  | validation codes of `Classification`, `Size`, `Money`, `PurchaseInfo`, `Garment` |
| `UpdateGarmentHandler`  | `UpdateGarmentCommand` (partial) | garment  | `garment.not_found`, `garment.archived_read_only`, validation codes              |
| `ArchiveGarmentHandler` | `GarmentId`                      | garment  | `garment.not_found`, `garment.already_archived`                                  |
| `RestoreGarmentHandler` | `GarmentId`                      | garment  | `garment.not_found`, `garment.not_archived`                                      |

A garment that exists but belongs to someone else is reported exactly like one that does not
exist.

Commands save once, through `IUnitOfWork`, after the domain accepted every change. A partial edit
uses `FieldUpdate<T>`: an unset field is left alone, a set field (even to `null`) is applied.
Time comes from an injected `TimeProvider`; "today", for the rule that a purchase cannot be in the
future, is the UTC date of the request. That never affects users west of UTC (the expected
audience); a user far east of UTC may be unable to record a same-day purchase for a few hours after
local midnight. Taking the user's time zone into account is left for when it matters.

## Persistence

`Buckl.Infrastructure/Persistence` holds the EF Core context (`BucklDbContext`), one record per
table under `Records/`, and one `IEntityTypeConfiguration` per record under `Configurations/`
([ADR-0023](../adr/0023-persist-aggregates-through-persistence-records.md)). Records are plain
rows; the domain aggregates never reach EF Core. Enumerations are stored as lower-case text
through `EnumText`, which also generates the value lists of the check constraints.

`Mapping/GarmentMapper` and `Mapping/ProductMapper` convert between aggregates and records.
Reading goes through the domain's `Rehydrate` factories, so a row that breaks an invariant fails
loudly instead of producing an invalid aggregate. Repositories under `Repositories/` implement the
domain ports; they stage changes and never call `SaveChanges` themselves.

`EfGarmentRepository` filters exactly like the web app's in-memory repository, so replacing it in
phase 6 changes no result: category and color by equality, size by case-insensitive equality, and
free text as a case-insensitive substring of one haystack joining the category, the color, the
notes, the size label, and the linked product's name and brand, in that order, skipping missing
values and joined with single spaces (matching the web app's `haystack`), with `%`, `_` and `\`
matched literally. Garments are ordered newest first.

The connection string is `ConnectionStrings:Buckl` and always uses the application role
(`buckl_app`). It is read the first time a context is built, so the API starts without it.

### Row-Level Security binding

`EfUserTransactionFactory` opens the request's transaction and runs
`select set_config('app.user_id', @userId, true)`, so the policies see that user until the
transaction ends ([ADR-0025](../adr/0025-bind-each-request-to-one-user-scoped-transaction.md)).
Handlers persist through `IUnitOfWork.SaveChangesAsync`, which writes inside that transaction;
they never commit. The integration tests prove that a committed user does not leak into the next
use of a pooled connection.

## Authentication

Every endpoint requires a valid Auth0 access token unless it opts out explicitly
([ADR-0028](../adr/0028-validate-auth0-access-tokens-with-jwt-bearer.md)).
`AddBucklAuthentication` registers ASP.NET Core's JWT bearer handler for the tenant in
`Auth0:Domain` and the API identifier in `Auth0:Audience`. Both settings are required in every
environment and validated when the host starts, so a missing or malformed value stops the API.

- Tokens must be RS256, signed by a key of the tenant's JWKS (downloaded from
  `https://<domain>/.well-known/openid-configuration` on the first authenticated request, cached
  and refreshed on rotation), issued by `https://<domain>/` for the configured audience, and within
  their lifetime (five minutes of clock skew).
- Claim names are kept as issued (`MapInboundClaims = false`): the subject is `sub`.
- A token without a subject, or with one longer than 255 characters, is rejected.
- A missing or invalid token gets `401` with `WWW-Authenticate: Bearer` and code
  `request.unauthenticated`. A policy beyond authentication that fails (none today) gets `403`
  with `request.forbidden`.

`IUserProvisioning` maps `sub` to the local user id, creating the `users` row the first time.

Tests host the API with a stand-in tenant (`TestTokens` in `Buckl.Api.Tests`): a signing key
generated per test run and its OpenID configuration, set as the handler's static metadata. The
validation code is the production one; only the source of the keys changes.

## Request pipeline

1. Exception handler and status code pages, so every error leaves as problem details (PR 4.13).
2. Authentication (JWT bearer, Auth0 access tokens) and authorization (authenticated by default).
3. `UserTransactionFilter`, a global MVC action filter: it maps the `sub` claim to the local user
   through `IUserProvisioning`, binds `ICurrentUser`, and opens the user-scoped transaction.
4. The controller action calls one handler; handlers save through `IUnitOfWork`.
5. Back in the filter, the transaction commits if the action completed, and rolls back if it
   threw.

The health endpoint is not an MVC action, so it never opens a transaction.

## Endpoints

The contract mirrors the web app's repository ports (`apps/web/src/domain`), so phase 6 swaps the
in-memory repository for HTTP without touching screens. JSON is camel case, enumerations are
lower-case strings, dates are `YYYY-MM-DD`, and timestamps are ISO 8601 in UTC.

| Method and path               | Purpose                                                                | Success          |
| ----------------------------- | ---------------------------------------------------------------------- | ---------------- |
| `GET /health`                 | Liveness probe (anonymous)                                             | 200              |
| `GET /garments`               | Wardrobe: `?category=&color=&size=&q=&status=`                         | 200              |
| `GET /garments/{id}`          | One garment                                                            | 200              |
| `GET /products/{id}`          | One catalog product                                                    | 200              |
| `POST /garments`              | Add a garment by hand                                                  | 201 + `Location` |
| `PATCH /garments/{id}`        | Partial edit: absent fields untouched, `null` clears                   | 200              |
| `POST /garments/{id}/archive` | Archive                                                                | 200              |
| `POST /garments/{id}/restore` | Restore                                                                | 200              |
| `GET /openapi/v1.json`        | OpenAPI document with the bearer token scheme (Development, anonymous) | 200              |
| `GET /scalar/v1`              | API reference UI (Development, anonymous)                              | 200              |

The OpenAPI document declares the bearer token as a security requirement of every operation, so
the API reference at `/scalar/v1` can send one.

`photoUrl` is always `null` until phase 6 attaches photos. Lists are not paginated in v1.

Request bodies are validated for shape only (required properties, known enumeration values);
ranges and formats are the domain's, so a rejected amount or currency comes back with the domain's
code. In `PATCH`, `classification` cannot be `null`. Write endpoints return the garment as stored,
so the web app never needs a second request.

## Errors

Every error is a problem details document (`application/problem+json`) with a `code` extension.

| Status | When                                                             | `code`                                             |
| ------ | ---------------------------------------------------------------- | -------------------------------------------------- |
| 400    | Malformed request: missing field, unknown enum value             | `request.invalid`                                  |
| 400    | Input a domain rule rejects                                      | the domain code, e.g. `money.negative_amount`      |
| 401    | No or invalid identity                                           | `request.unauthenticated`                          |
| 404    | Unknown route                                                    | `resource.not_found`                               |
| 404    | Garment or product the caller cannot see                         | `garment.not_found`, `product.not_found`           |
| 409    | A rule about the current state, e.g. editing an archived garment | the domain code, e.g. `garment.archived_read_only` |
| 500    | Anything unexpected; no internal details are returned            | `server.error`                                     |

## Running locally

`dotnet run --project src/Buckl.Api` from `apps/api` listens on `http://localhost:5080`.
