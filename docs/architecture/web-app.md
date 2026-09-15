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

| Path                        | Screen                   |
| --------------------------- | ------------------------ |
| `/`                         | Redirects to `/wardrobe` |
| `/wardrobe`                 | Wardrobe list            |
| `/wardrobe/:garmentId`      | Garment detail           |
| `/wardrobe/:garmentId/edit` | Garment edit             |
| `/garments/new`             | Add garment              |
| `*`                         | Not found                |

The shell (`AppLayout`) renders a header, a main navigation (bottom bar on phones, sidebar from
768px) and the active page in an `Outlet`. Every page sets the document title with
`usePageTitle`.

## Design system

Tokens live in `src/styles/tokens.css` and base styles in `src/styles/base.css`
([ADR-0018](../adr/0018-style-with-css-modules-and-design-tokens.md)). Base components in
`src/components` are the only place that knows the raw markup of a control:

| Component    | Purpose                                                                      |
| ------------ | ---------------------------------------------------------------------------- |
| `Button`     | Primary, secondary and danger actions; `type="button"` unless told otherwise |
| `Input`      | Labelled text input with hint and error bound by `aria-describedby`          |
| `Select`     | Labelled select with an optional placeholder option                          |
| `TextArea`   | Labelled multi-line input                                                    |
| `Card`       | Surface with border, radius and shadow                                       |
| `EmptyState` | Titled region with a description and a call to action                        |

Feature components use tokens only, never raw colors or sizes.
