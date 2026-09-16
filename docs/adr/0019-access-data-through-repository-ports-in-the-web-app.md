# ADR-0019: Access data in the web app through repository ports

- **Status:** Accepted
- **Date:** 2026-09-15
- **Deciders:** NahuelFernandezGalli

## Context

Phase 3 builds every screen before the API exists (phase 4) and before the front end can call it
(phase 6). Screens still need realistic data, filtering, creation, edition and archiving, and the
behavior scenarios need deterministic fixtures. When the API arrives, the screens and their
scenarios should not change.

## Decision

Screens never fetch data themselves. They consume `GarmentRepository` and `ProductRepository`,
TypeScript interfaces in `apps/web/src/domain` that mirror the domain ports `IGarmentRepository`
and `IProductRepository` plus the use cases the API will expose (`create`, `update`, `archive`,
`restore`). Implementations live in `apps/web/src/data`: `InMemoryGarmentRepository` in phase 3,
an HTTP-backed repository in phase 6. `RepositoriesProvider` injects them at the root; pages get
them with `useRepositories()` and load data through small hooks (`useWardrobe`, `useGarment`)
that expose a discriminated state (`loading`, `ready`, `error`, `not-found`).

The in-memory repository enforces the domain rules the UI must react to (an archived garment is
read-only, archiving twice fails) and raises typed errors with the same codes the domain uses, so
the UI can be built against the real failure modes now.

The front-end types are a read model of what the API returns, not a copy of the aggregates: no
`ownerId` (isolation is the API's job through RLS), `photoUrl` instead of `photoKey` (the API
serves signed URLs), and enumerations as the lower-case text the database stores (ADR-0016).

## Alternatives considered

- **Mock the HTTP layer (MSW) from the start** — closer to production, but it forces the API
  contract to be designed now, before phase 4 settles it, and every scenario would speak HTTP.
- **A server-state library (TanStack Query) with mocked fetchers** — good caching, but a new
  dependency whose value appears only with a real network; reconsider in phase 6.
- **Global store (Redux, Zustand) holding the wardrobe** — duplicates the repository's state and
  couples screens to the store's shape.

## Consequences

### Positive

- Screens and scenarios are written once; phase 6 swaps the implementation behind the provider.
- Tests inject a repository seeded with exactly the garments a scenario names.
- The rules a screen must respect are visible in the port's typed errors.

### Negative

- Two type definitions of the same model (C# and TypeScript) kept in sync by hand until phase 6
  generates types from OpenAPI.
- The in-memory repository loses its state on reload; acceptable for a phase 3 demo.
