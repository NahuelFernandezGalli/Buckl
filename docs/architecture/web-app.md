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
| `src/data`       | Implementations of the ports: HTTP for the app, in-memory for tests.                                         |
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

| Where | Implementation                                                                                               |
| ----- | ------------------------------------------------------------------------------------------------------------ |
| App   | `HttpGarmentRepository` and `HttpProductRepository` over `requestJson`; photos through `createPhotoUploader` |
| Tests | `InMemoryGarmentRepository` and `InMemoryProductRepository`, injected by `renderApp`                         |

`App` builds the HTTP repositories inside the session provider (`ApiRepositoriesProvider`), once for
the life of the app; tokens come from the latest session. The app has no sample data any more; a new
user starts with an empty wardrobe.

Lookups answer `null` for a 404, like the in-memory repositories; `update` sends only the fields
present in `GarmentChanges` (`PATCH`), and a `Blob` photo is uploaded first and sent as `{ uploadId }`
([ADR-0034](../adr/0034-mirror-the-api-contract-by-hand-in-the-web-app.md)).

The types in `src/domain` are a read model of the API: `photoUrl` is the URL the browser can load
(a signed URL from the API, a data URL in the in-memory repository), enumerations are the
lower-case text stored in the database, and `purchaseInfo.date` is a `YYYY-MM-DD` string.
Timestamps are ISO 8601 in UTC. `src/lib/format.ts` formats money and purchase dates for display.

Photos: `PhotoCapture` previews a picked file through an object URL. On save, the form hands the
file to its page as a `Blob`, which crosses the port unchanged (`NewGarment.photo`,
`GarmentChanges.photo`, where leaving it out keeps the current photo and `null` removes it).
`InMemoryGarmentRepository` turns the blob into a data URL and serves it as `photoUrl`. The HTTP
repository prepares the photo first (`preparePhoto`,
[ADR-0033](../adr/0033-prepare-photos-in-the-browser-before-upload.md)): the centered 4:5 part, at
most 1080×1350, as a JPEG without metadata. Then it uploads to Backblaze B2 through a presigned
URL, with no change to the screens. `createPhotoUploader` is the browser's side of
[ADR-0032](../adr/0032-upload-photos-straight-to-storage-through-a-staging-prefix.md): it prepares
the photo, asks `POST /photos/uploads` for a ticket, and PUTs the photo straight to storage with the
ticket's headers and without the access token. A failed PUT is a `PhotoUploadError`. Garments
without a photo show `placeholderPhoto(color)`, a flat SVG.

`src/data/api/api-client.ts` is the only way to reach the API: `createApiClient`
asks the session for an access token on every request, sends it as `Authorization: Bearer`, and
accepts only paths relative to the API root, so the token never travels to another origin. A
`SessionExpiredError` from the session stops the request before it is sent. The HTTP repositories
sit on top of it, and `src/data/api/responses.ts` builds on it:
`requestJson` sends and reads JSON and sorts failures into the three the screens treat differently:
`SessionExpiredError` (no valid token, including a 401 from the API), `NetworkError` (no answer)
and `ApiError` (an error answer, with the API's stable `code`).

## Loading and errors

Screens show a loading status while a repository call runs, and a message when it fails.
`describeError` (`src/app/describe-error.ts`) turns a failure into words a person can act on, by
the stable `code` the API, the domain mirror and the photo pipeline attach to their errors: an
expired session, no connection, a server fault, an archived garment, and every photo failure.
Loading the wardrobe or a garment offers "Try again". A failed save keeps the form as it was, with
the reason above the save button, which reads "Saving…" while it waits. A photo that does not load
(an expired URL, storage unreachable) shows the garment's color instead, and a fresh URL is tried
the next time the screen loads.

The service worker still caches only the app shell: API responses (`Cache-Control: no-store`) and
photos (signed URLs that change on every response) are never cached by it, which settles what
[ADR-0020](../adr/0020-use-vite-plugin-pwa-for-the-installable-shell.md) left to this phase.

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

The SDK keeps reporting a signed-in user from its cached profile after the refresh token expires
(ADR-0030). The API is what notices: a 401, or a session that cannot renew its token, turns on a
notice in the layout, "Your session expired", with "Log in again", which comes back to the same
address. It never redirects on its own, so a half-filled form is not lost and a misconfigured API
cannot cause a login loop.

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
