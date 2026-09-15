# ADR-0017: Use React Router in data mode for client-side routing

- **Status:** Accepted
- **Date:** 2026-09-15
- **Deciders:** NahuelFernandezGalli

## Context

The web app is a static single-page application (ADR-0006) with a handful of screens: wardrobe,
garment detail, garment creation and edition, and later outfits. Screens need addressable URLs so
the phone's back button works, a garment can be shared as a link, and wardrobe filters survive a
reload. Tests must be able to render any screen at any address without a browser.

## Decision

We use `react-router` (version 8, data mode) for client-side routing. Routes are declared once as
`RouteObject[]` in `apps/web/src/app/routes.tsx`, mounted in the app with `createBrowserRouter`
and in tests with `createMemoryRouter`, so a scenario can start at any address. Wardrobe filters
live in the query string. The app does not use loaders or actions: data comes from repository
ports (ADR-0019) consumed by hooks inside each page, which keeps data access independent from the
router and easier to swap in phase 6.

## Alternatives considered

- **TanStack Router** — excellent type safety, but a larger API surface and file-based
  conventions that add little for six routes.
- **wouter** — tiny, but no memory router for tests and no nested layouts out of the box.
- **Hand-rolled routing on `history`** — no dependency, but re-implements nested routes,
  active links and memory routing that the tests need.

## Consequences

### Positive

- One route table shared by the app and the tests.
- Nested layout (`AppLayout` with `Outlet`) gives the shell for free.
- `NavLink` marks the active section with `aria-current` without extra code.

### Negative

- One runtime dependency with its own release cadence; Dependabot keeps it current.
- Loaders and actions are deliberately unused, so the router's data features stay idle.
