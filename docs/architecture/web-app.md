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
| `src/session`    | The session port, its Auth0 adapter and the return-path guard (phase 5).                                     |
| `src/styles`     | Design tokens and base styles (phase 3, PR 3.2).                                                             |
| `src/test`       | Test helpers: `renderApp`, object mothers, global setup.                                                     |

## Routing

Routes are declared in `src/app/routes.tsx` and mounted by `createBrowserRouter` in `App`
([ADR-0017](../adr/0017-use-react-router-for-client-side-routing.md)). Tests mount the same table
with `createMemoryRouter` through `renderApp({ route })`.

| Path                                          | Screen                                                                                             |
| --------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `/welcome`                                    | Welcome screen with "Log in"; public. A signed-in user is sent on to the wardrobe                  |
| `/callback`                                   | Where Auth0 returns after a login; public. Shows "Signing you in…" while the SDK finishes          |
| `/`                                           | Redirects to `/wardrobe`                                                                           |
| `/wardrobe?category=&color=&size=&q=&status=` | Wardrobe list; every filter is a query parameter so the address is shareable and survives a reload |
| `/wardrobe/:garmentId`                        | Garment detail                                                                                     |
| `/wardrobe/:garmentId/edit`                   | Garment edit                                                                                       |
| `/garments/new`                               | Add garment                                                                                        |
| `*`                                           | Not found                                                                                          |

Every other route sits under the `RequireSession` layout route: while the session loads it shows a
loading message, and a signed-out visitor is sent to `/welcome`, which remembers the address they
asked for and returns to it after the login. The header shows the signed-in user's name and a "Log
out" button.

The shell (`AppLayout`) renders a header, a main navigation (bottom bar on phones, sidebar from
768px) and the active page in an `Outlet`. Every page sets the document title with
`usePageTitle`.

## Data access

Screens depend on the ports in `src/domain/garment-repository.ts` and
`src/domain/product-repository.ts` ([ADR-0019](../adr/0019-access-data-through-repository-ports-in-the-web-app.md)).
`RepositoriesProvider` injects an implementation at the root; `useRepositories()` returns it.

| Phase | Implementation                                                                                |
| ----- | --------------------------------------------------------------------------------------------- |
| 3     | `InMemoryGarmentRepository` and `InMemoryProductRepository`, seeded with `sample-wardrobe.ts` |
| 6     | Repository over the typed HTTP client; in-memory stays for tests                              |

The types in `src/domain` are a read model of the API: `photoUrl` is the URL the browser can load
(a signed URL from the API, a data URL while the app runs on mock data), enumerations are the
lower-case text stored in the database, and `purchaseInfo.date` is a `YYYY-MM-DD` string.
Timestamps are ISO 8601 in UTC. `src/lib/format.ts` formats money and purchase dates for display.

Photos in phase 3: `PhotoCapture` previews a picked file through an object URL. On save, the form
hands the file to its page as a `Blob`, which crosses the port unchanged (`NewGarment.photo`,
`GarmentChanges.photo`, where leaving it out keeps the current photo and `null` removes it).
`InMemoryGarmentRepository` turns the blob into a data URL and serves it as `photoUrl`; in phase 6
the HTTP repository uploads it to R2 through a presigned URL instead, with no change to the screens.
Sample garments use `placeholderPhoto(color)`, a flat SVG.

From phase 5, `src/data/api/api-client.ts` is the only way to reach the API: `createApiClient`
asks the session for an access token on every request, sends it as `Authorization: Bearer`, and
accepts only paths relative to the API root, so the token never travels to another origin. A
`SessionExpiredError` from the session stops the request before it is sent. Phase 6 builds the
typed client and the HTTP repositories on top of it.

## Session

Screens learn who is signed in through the `Session` port in `src/session/session.ts`
([ADR-0029](../adr/0029-handle-sign-in-through-a-session-port-in-the-web-app.md)) and
`useSession()`. `Auth0SessionProvider` implements it over `@auth0/auth0-react`:

| Port               | Auth0                                                                  |
| ------------------ | ---------------------------------------------------------------------- |
| `state: loading`   | the SDK is restoring the session or finishing a login                  |
| `state: signedIn`  | authenticated; `displayName` is the profile name, or the email         |
| `state: signedOut` | not authenticated; `error` is set after a failed login                 |
| `signIn(returnTo)` | `loginWithRedirect` to Auth0, back to `/callback`, then to `returnTo`  |
| `signOut()`        | `logout`, back to `/welcome`                                           |
| `getAccessToken()` | `getAccessTokenSilently`; `SessionExpiredError` when a login is needed |

Tokens are cached in local storage and renewed with rotating refresh tokens
([ADR-0030](../adr/0030-keep-the-web-session-with-rotating-refresh-tokens.md)). `safeReturnTo`
accepts only paths inside the app as the destination after a login.

## Design system

Tokens live in `src/styles/tokens.css` and base styles in `src/styles/base.css`
([ADR-0018](../adr/0018-style-with-css-modules-and-design-tokens.md)). Base components in
`src/components` are the only place that knows the raw markup of a control:

| Component       | Purpose                                                                                                                                  |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `Button`        | Primary, secondary and danger actions; `type="button"` unless told otherwise; forwards its `ref`                                         |
| `ButtonLink`    | A router `Link` styled as a `Button`, for navigation that reads as an action                                                             |
| `Field`         | `useFieldDescription` and `FieldMessages`: the id, hint and error wiring (`aria-describedby`) shared by `Input`, `Select` and `TextArea` |
| `Input`         | Labelled text input with hint and error bound by `aria-describedby`                                                                      |
| `Select`        | Labelled select with an optional placeholder option                                                                                      |
| `TextArea`      | Labelled multi-line input                                                                                                                |
| `Card`          | Surface with border, radius and shadow                                                                                                   |
| `EmptyState`    | Titled region with a description and a call to action                                                                                    |
| `ConfirmDialog` | Inline `alertdialog` for destructive actions; focus lands on the confirm button, Escape cancels                                          |

In feature components, colors, type sizes, spacing and radii come from tokens; component-specific
layout dimensions (max widths, photo sizes, touch-target minimums) may be literal values.

## Installable shell

`vite-plugin-pwa` generates `manifest.webmanifest` and a Workbox service worker at build time
([ADR-0020](../adr/0020-use-vite-plugin-pwa-for-the-installable-shell.md)). The service worker
precaches the build output only, so the app shell opens offline; garments and photos are not
cached by it. Icons come from `public/icon.svg` through `npm run pwa:assets -w web`.
