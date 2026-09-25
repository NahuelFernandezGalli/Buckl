# ADR-0026: Authenticate with a development scheme until Auth0 arrives

- **Status:** Superseded by [ADR-0028](0028-validate-auth0-access-tokens-with-jwt-bearer.md)
- **Date:** 2026-09-22
- **Deciders:** NahuelFernandezGalli

## Context

Phase 4 must prove Row-Level Security end to end and run the garment CRUD from localhost, but
Auth0 arrives in phase 5. Everything downstream of authentication (the local user record, the
user-scoped transaction, the current user in handlers) only needs a stable subject per caller.
Building that pipeline now, against a stand-in identity, keeps phase 5 down to swapping one
authentication scheme.

## Decision

Until phase 5, the API authenticates with a `Development` scheme that reads the subject from the
`X-Dev-User` header and emits it as the `sub` claim, the same claim the Auth0 access token
carries. Every endpoint requires an authenticated user through the fallback authorization policy;
only the health endpoint and the development API reference opt out. The subject goes through the
same path a real one will: `IUserProvisioning` upserts the `users` row and returns the local
`UserId`.

The scheme exists only in the Development environment, and the API refuses to start in any other
environment until phase 5 replaces it with JWT bearer validation.

## Alternatives considered

- **A fixed development user from configuration** — simpler today, but phase 5 would have to add
  authentication, the fallback policy and user provisioning all at once, and two users could not
  be tested against each other.
- **No HTTP identity in phase 4; tests only** — the exit criterion asks for the CRUD from
  localhost.
- **Auth0 now** — pulls phase 5 into phase 4 and makes the RLS work depend on an external tenant.

## Consequences

### Positive

- Two local users (`dev|alice`, `dev|bob`) can be exercised by hand and by tests.
- Phase 5 replaces one registration; the filter, provisioning and handlers do not change.

### Negative

- Anyone who can reach a Development instance can act as any subject. The guard against running
  outside Development is the only thing standing between this scheme and production, and it is
  covered by a test.
