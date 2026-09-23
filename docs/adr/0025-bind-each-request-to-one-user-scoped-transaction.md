# ADR-0025: Bind each request to one user-scoped transaction

- **Status:** Accepted
- **Date:** 2026-09-22
- **Deciders:** NahuelFernandezGalli

## Context

Row-Level Security reads the current user from the `app.user_id` session variable (ADR-0007). A
transaction-local setting is the only safe kind with connection pooling, because it disappears
when the transaction ends; a session-level setting would follow the pooled connection into the
next request. A transaction-local setting also means reads need a transaction, not only writes,
and the question is who opens it: every use case, or the request.

## Decision

Every authenticated request runs inside exactly one database transaction bound to its user.
`IUserTransactionFactory.BeginAsync(userId)` opens the transaction on the request's `DbContext`
and executes `select set_config('app.user_id', @userId, true)`, the parameterizable equivalent of
`SET LOCAL`. A global MVC action filter (`UserTransactionFilter`, PR 4.12) resolves the user,
begins the transaction before the action runs and commits it only if the action completed without
an exception; otherwise disposing the transaction rolls it back.

Handlers never see the transaction. Repositories stage changes, and handlers call
`IUnitOfWork.SaveChangesAsync` once, which writes inside the already open transaction.

## Alternatives considered

- **Each use case opens its own unit of work** — finer control, but every new handler, including
  every read, has to remember it, and a forgotten one silently returns no rows.
- **A connection interceptor that sets the variable for the session** — no transaction needed for
  reads, but the value outlives the request on a pooled connection unless it is reset perfectly
  every time, and a transaction-mode pooler (Neon's) does not keep sessions at all.
- **Filtering by user in every query** — kept as a second layer (`ListByOwnerAsync` receives the
  owner), never as the only one.

## Consequences

### Positive

- A new endpoint is isolated by construction: it cannot run a query outside a user transaction.
- A failure anywhere in the action rolls back everything the request wrote.

### Negative

- Each request holds one connection and one open transaction for its whole duration; slow work
  (phase 7 imports) must happen before or outside the database part of a request.
- Endpoints that must not be bound to a user need an explicit opt-out when they appear.
