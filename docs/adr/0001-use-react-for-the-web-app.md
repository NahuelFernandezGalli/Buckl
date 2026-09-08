# ADR-0001: Use React for the web app

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

Buckl is a phone-first app delivered as a PWA and as a regular web site, built by one person as a
portfolio project. The front end has to be productive to iterate on screens before the API exists,
have a mature testing story, and be recognizable to reviewers of the portfolio.

## Decision

We use React with TypeScript, built by Vite and tested with Vitest and Testing Library. The app is
a single-page application that will be packaged as a PWA (manifest and service worker) in phase 3.

## Alternatives considered

- **Angular** — the other option the author considered. A full framework with opinionated
  structure, but heavier for a small app, slower to iterate on screens, and its tooling story for
  PWA plus Vite-style builds is less direct. Rejected for this project, not in general.
- **Vue or Svelte** — viable, but less common in the target job market and not worth the learning
  cost here.

## Consequences

### Positive

- Fast feedback loop (Vite hot reload, Vitest) while designing screens against mock data.
- Large ecosystem for routing, forms, PWA tooling and Auth0's SPA SDK.
- Behavior tests with Testing Library match how users interact with the UI.

### Negative

- React does not prescribe structure; conventions (folders by feature, hooks for data access) must
  be set and kept by hand.
- Two languages in the repository, TypeScript and C#, so domain types are mirrored in the front
  end.
