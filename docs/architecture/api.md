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
free text as a case-insensitive substring of the category, the color, the notes, the size label,
or the linked product's name or brand, with `%`, `_` and `\` matched literally. Garments are
ordered newest first.

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

Every endpoint requires an authenticated user unless it opts out explicitly. Until phase 5 the
only scheme is `Development`
([ADR-0026](../adr/0026-authenticate-with-a-development-scheme-until-auth0.md)): the
`X-Dev-User: <subject>` header authenticates the request as that subject, and the API refuses to
start outside the Development environment. The subject travels as the `sub` claim, as it will in
Auth0 access tokens. `IUserProvisioning` maps it to the local user id, creating the `users` row
the first time.

## Request pipeline

1. Exception handler and status code pages, so every error leaves as problem details (PR 4.13).
2. Authentication (`Development` scheme until phase 5) and authorization (authenticated by
   default).
3. `UserTransactionFilter`, a global MVC action filter: it maps the `sub` claim to the local user
   through `IUserProvisioning`, binds `ICurrentUser`, and opens the user-scoped transaction.
4. The controller action calls one handler; handlers save through `IUnitOfWork`.
5. Back in the filter, the transaction commits if the action completed, and rolls back if it
   threw.

The health endpoint is not an MVC action, so it never opens a transaction.

## Endpoints

| Method and path | Authentication | Purpose                            |
| --------------- | -------------- | ---------------------------------- |
| `GET /health`   | anonymous      | Liveness probe for hosts and tests |

## Running locally

`dotnet run --project src/Buckl.Api` from `apps/api` listens on `http://localhost:5080`.
