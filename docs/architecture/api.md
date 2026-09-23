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

## Endpoints

| Method and path | Authentication | Purpose                            |
| --------------- | -------------- | ---------------------------------- |
| `GET /health`   | anonymous      | Liveness probe for hosts and tests |

## Running locally

`dotnet run --project src/Buckl.Api` from `apps/api` listens on `http://localhost:5080`.
