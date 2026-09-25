# ADR-0029: Handle sign-in in the web app through a session port

- **Status:** Accepted
- **Date:** 2026-09-25
- **Deciders:** NahuelFernandezGalli

## Context

Screens need to know whether someone is signed in, start a login or a logout, and, from phase 6,
get an access token for the API. The Auth0 SDK exposes all of that through a React hook tied to
its provider, which scenario tests would have to mock everywhere. Data access already goes through
ports ([ADR-0019](0019-access-data-through-repository-ports-in-the-web-app.md)).

## Decision

We give the web app a `Session` port (`src/session/session.ts`): a state (`loading`,
`signedOut` with an optional error, `signedIn` with a display name), `signIn(returnTo)`,
`signOut()` and `getAccessToken()`. `Auth0SessionProvider` implements it over
`@auth0/auth0-react`; screens only call `useSession()`.

- `/welcome` and `/callback` are public routes. Every other screen sits under a `RequireSession`
  layout route, which waits while the session loads and sends a signed-out visitor to `/welcome`,
  remembering the address they asked for.
- Auth0 always returns to `/callback`. The in-app destination travels in the SDK's `appState` and
  is accepted only if `safeReturnTo` says it is a path inside the app.
- An expired session surfaces as `SessionExpiredError` from `getAccessToken`, so callers can ask
  for a new login instead of showing a generic failure.
- Scenario tests render the app with `fakeSession`; only the adapter's own test mocks the SDK.

## Alternatives considered

- **`useAuth0` in every component** — couples screens to the vendor and forces a module mock in
  every scenario.
- **`withAuthenticationRequired`** — sends a signed-out visitor straight to Auth0, with no screen
  of our own to explain what Buckl is or why a login failed.
- **Returning to `/`** — the index route redirects to the wardrobe and could drop the authorization
  code before the SDK reads it.

## Consequences

### Positive

- Scenarios describe signed-in, signed-out, loading and failed states without knowing Auth0.
- Replacing the identity provider touches one adapter.
- Open redirects after login are ruled out in one tested function.

### Negative

- One more indirection over the SDK; features of the SDK we want later (popups, MFA prompts) need
  a method on the port first.
