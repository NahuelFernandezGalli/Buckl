# ADR-0004: Use Neon for Postgres hosting

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

[ADR-0002](0002-use-dotnet-api-with-postgres.md) commits to Postgres. Buckl needs a managed
instance that costs nothing at portfolio scale, supports standard Postgres features (RLS, roles,
extensions) and can be reached from localhost during development and from the API host later.

## Decision

We host Postgres on Neon's free tier: one project with a development branch and a production
branch. The API connects with a dedicated application role that cannot bypass RLS.

## Alternatives considered

- **Supabase (database only)** — also free managed Postgres, but bundling a platform we do not use
  otherwise adds surface. Neon is Postgres and nothing else.
- **Local Postgres only** — fine for development, but it blocks the free public deployment.
- **Railway or Render Postgres** — their free tiers are smaller or expire.

## Consequences

### Positive

- Standard Postgres, so RLS, roles and migrations work exactly as documented.
- Branching gives a free development database separate from production.

### Negative

- Free tier compute suspends when idle, so the first request after a pause is slow.
- Connection limits favor pooling, which makes EF Core pooling and short transactions matter.
