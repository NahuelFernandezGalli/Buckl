# ADR-0007: Enforce tenant isolation with Row-Level Security via a session variable

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

Each user must see only their own garments and outfits. Filtering in application code alone fails
silently when a query forgets its `WHERE user_id = ...`. The database can enforce the rule for
every query regardless of who wrote it.

## Decision

Every user-scoped table has Row-Level Security enabled and forced, with policies that compare
`user_id` against `current_setting('app.user_id', true)::uuid`. The API sets that variable with
`SET LOCAL app.user_id = '<uuid>'` at the start of each request's transaction, using the
authenticated user's local id. The application role cannot bypass RLS. The mechanism is detailed
in [Authentication and security](../architecture/auth-and-security.md).

## Alternatives considered

- **Application-level filtering only** — the simplest option, but one missing filter leaks data.
  It is kept as an additional layer, not as the only one.
- **One database role per user** — the strongest isolation, but impossible to manage with
  connection pooling and many users.
- **A schema per user** — heavy for migrations, with no benefit at this scale.

## Consequences

### Positive

- Isolation holds even if application code has a bug, and it is provable with integration tests.
- No per-user credentials: one pooled connection role.

### Negative

- Every request needs a transaction and the `SET LOCAL`. Forgetting it yields empty results, which
  is safe but confusing, so the interceptor must be covered by tests.
- Migrations and the application must use different roles.
