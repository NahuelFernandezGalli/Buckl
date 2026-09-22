# ADR-0020: Use vite-plugin-pwa for the installable shell

- **Status:** Accepted
- **Date:** 2026-09-15
- **Deciders:** NahuelFernandezGalli

## Context

Buckl is meant to be used from the phone as an installed app (ADR-0001, ADR-0006): that is
where the camera is and where product links get shared. Installability needs a web app manifest,
icons in several sizes and a service worker that serves the app shell. Writing and maintaining a
service worker by hand is error-prone (stale caches, broken updates), and the app shell changes
with every build.

## Decision

We generate the manifest and the service worker at build time with `vite-plugin-pwa`, which
wraps Workbox. The service worker precaches only the build output (the app shell) and falls back
to `index.html` for navigations, so the app opens offline at its last installed version. It
updates automatically on the next visit (`registerType: 'autoUpdate'`) and is registered by a
script the plugin injects into `index.html`, so no application code depends on it and the test
suite runs with the plugin disabled. Icons are generated from `public/icon.svg` with
`@vite-pwa/assets-generator` and committed.

Data (garments, photos) is never cached by this service worker. Whether and how API responses
are cached is a phase 6 decision, once the real data flow exists.

## Alternatives considered

- **Hand-written service worker** — full control, but cache versioning and update handling are
  exactly the bugs a generator avoids, and the shell's file list changes every build.
- **Workbox CLI outside Vite** — same engine, but a second build step to keep in sync with
  Vite's output.
- **No PWA, browser only** — simpler, but it gives up the home-screen icon, standalone window
  and offline shell that make the phone experience feel like an app.

## Consequences

### Positive

- Installable on Android and iOS with a home-screen icon and a standalone window.
- The shell opens offline; updates roll out on the next visit with no manual step.
- Nothing in `src/` knows about the service worker; tests are unaffected.

### Negative

- One more build-time dependency (Workbox) to keep current.
- Auto-update means a user mid-session can get the new version on their next navigation; a
  visible "update available" prompt can replace it later if that proves disruptive.
- Testing installability needs a secure origin, which means `localhost` through `adb reverse`
  or a tunnel rather than a plain LAN address.
