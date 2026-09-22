# ADR-0022: Layer the API with controllers and one handler per use case

- **Status:** Accepted
- **Date:** 2026-09-22
- **Deciders:** NahuelFernandezGalli

## Context

Phase 4 turns the domain of phase 2 into an HTTP API backed by Postgres. The overview already
commits to a layered architecture with dependencies pointing inward. What remained open was the
shape of each layer: how HTTP reaches a use case, where dependencies are wired, and what stops a
layer from reaching into the wrong one as the code grows.

## Decision

The solution has four production projects: `Buckl.Domain` (no dependencies), `Buckl.Application`
(use cases and ports, depends on the domain), `Buckl.Infrastructure` (EF Core adapters, depends on
the application) and `Buckl.Api` (HTTP surface and composition root, depends on both). Each layer
exposes one registration method, `AddApplication()` and `AddInfrastructure(configuration)`, and
only `Program.cs` calls them.

HTTP endpoints are MVC controllers with `[ApiController]`, attribute routing and DataAnnotations
for request shape. Every use case is one handler class (`ListWardrobeHandler`,
`CreateGarmentHandler`) with a single `HandleAsync` method, registered as a scoped service and
injected into the action with `[FromServices]`. There is no mediator: a controller action calls
its handler directly. Handlers return domain objects; controllers map them to response contracts.

Architecture tests (ArchUnitNET plus reflection over referenced assemblies) fail the build when a
layer depends on one it must not, and when the domain or the application reference a persistence
or web framework.

## Alternatives considered

- **Minimal APIs** — less ceremony, but controllers keep routing, filters and model validation in
  one well-known model, and a global action filter is the natural place for the per-request
  transaction.
- **A mediator (MediatR or a source-generated one)** — pipeline behaviors for cross-cutting
  concerns, but an extra indirection for a handful of use cases, and MediatR moved to a commercial
  license in 2025. Cross-cutting concerns live in ASP.NET Core filters instead.
- **One project with folders per layer** — fewer projects, but nothing enforces the direction of
  dependencies except discipline.

## Consequences

### Positive

- Each use case is a small class that can be tested with hand-written fakes.
- A wrong reference between layers fails CI.

### Negative

- More projects and more wiring than a single-project API.
- Handlers are resolved per action with `[FromServices]`, so a controller's constructor does not
  show all its dependencies in one place.
