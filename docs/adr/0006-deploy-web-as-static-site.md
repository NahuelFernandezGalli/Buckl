# ADR-0006: Deploy the web app as a static site

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

The web app is a single-page PWA with no server-side rendering needs. Deployment must be free,
provide HTTPS and a custom domain, and be simple enough to automate from `main`.

## Decision

We build the web app to static assets with Vite and host them on a static site service, Cloudflare
Pages by default, though any equivalent with free HTTPS works. All dynamic behavior goes through
the API.

## Alternatives considered

- **Server-side rendering (Next.js, Remix)** — needs a Node runtime host and brings no benefit to
  an authenticated, personal app.
- **Serving the single-page app from the .NET API** — couples front and back deployments and moves
  static traffic onto the scarcest free resource, the API host.

## Consequences

### Positive

- Free, fast and globally cached; deploying is uploading the build output.
- A clear separation: the API only serves JSON.

### Negative

- Every secret must stay out of the bundle, as described in
  [Authentication and security](../architecture/auth-and-security.md).
- Client-side routing needs a fallback rewrite to `index.html` on the host.
