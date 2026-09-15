# Web app

How `apps/web` is organized and how a screen gets its data. The stack is React 19, Vite,
TypeScript and Vitest; testing follows [the testing strategy](../testing.md).

## Layout

| Folder           | Responsibility                                                                                               |
| ---------------- | ------------------------------------------------------------------------------------------------------------ |
| `src/app`        | Composition: routes, layout shell, providers, page title. Nothing here knows about a specific screen's data. |
| `src/features/*` | One folder per user-facing feature: its `.feature` scenarios, step definitions, page and components.         |
| `src/components` | Base components of the design system (phase 3, PR 3.2).                                                      |
| `src/domain`     | TypeScript mirror of the domain types and repository ports (phase 3, PR 3.3).                                |
| `src/data`       | Implementations of the ports: in-memory in phase 3, HTTP from phase 6.                                       |
| `src/lib`        | Pure helpers with unit tests (formatting, dates, files).                                                     |
| `src/styles`     | Design tokens and base styles (phase 3, PR 3.2).                                                             |
| `src/test`       | Test helpers: `renderApp`, object mothers, global setup.                                                     |

## Routing

Routes are declared in `src/app/routes.tsx` and mounted by `createBrowserRouter` in `App`
([ADR-0017](../adr/0017-use-react-router-for-client-side-routing.md)). Tests mount the same table
with `createMemoryRouter` through `renderApp({ route })`.

| Path                                          | Screen                                                                                             |
| --------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `/`                                           | Redirects to `/wardrobe`                                                                           |
| `/wardrobe?category=&color=&size=&q=&status=` | Wardrobe list; every filter is a query parameter so the address is shareable and survives a reload |
| `/wardrobe/:garmentId`                        | Garment detail                                                                                     |
| `/wardrobe/:garmentId/edit`                   | Garment edit                                                                                       |
| `/garments/new`                               | Add garment                                                                                        |
| `*`                                           | Not found                                                                                          |

The shell (`AppLayout`) renders a header, a main navigation (bottom bar on phones, sidebar from
768px) and the active page in an `Outlet`. Every page sets the document title with
`usePageTitle`.

## Data access

Screens depend on the ports in `src/domain/garment-repository.ts` and
`src/domain/product-repository.ts` ([ADR-0019](../adr/0019-access-data-through-repository-ports-in-the-web-app.md)).
`RepositoriesProvider` injects an implementation at the root; `useRepositories()` returns it.

| Phase | Implementation                                                   |
| ----- | ---------------------------------------------------------------- |
| 3     | `InMemoryGarmentRepository` seeded with `sample-wardrobe.ts`     |
| 6     | Repository over the typed HTTP client; in-memory stays for tests |

The types in `src/domain` are a read model of the API: `photoUrl` is the URL the browser can load
(a signed URL from the API, a data URL while the app runs on mock data), enumerations are the
lower-case text stored in the database, and `purchaseInfo.date` is a `YYYY-MM-DD` string.
Timestamps are ISO 8601 in UTC and are formatted to local time only in `src/lib/format.ts`.

Photos in phase 3: `PhotoCapture` previews a picked file through an object URL; on save, the form
turns the file into a data URL stored as `photoUrl`. Sample garments use `placeholderPhoto(color)`,
a flat SVG. Both are replaced by presigned uploads to R2 in phase 6.

## Design system

Tokens live in `src/styles/tokens.css` and base styles in `src/styles/base.css`
([ADR-0018](../adr/0018-style-with-css-modules-and-design-tokens.md)). Base components in
`src/components` are the only place that knows the raw markup of a control:

| Component       | Purpose                                                                                         |
| --------------- | ----------------------------------------------------------------------------------------------- |
| `Button`        | Primary, secondary and danger actions; `type="button"` unless told otherwise                    |
| `Input`         | Labelled text input with hint and error bound by `aria-describedby`                             |
| `Select`        | Labelled select with an optional placeholder option                                             |
| `TextArea`      | Labelled multi-line input                                                                       |
| `Card`          | Surface with border, radius and shadow                                                          |
| `EmptyState`    | Titled region with a description and a call to action                                           |
| `ConfirmDialog` | Inline `alertdialog` for destructive actions; focus lands on the confirm button, Escape cancels |

Feature components use tokens only, never raw colors or sizes.
