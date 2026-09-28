# ADR-0028: Validate Auth0 access tokens with JWT bearer authentication

- **Status:** Accepted
- **Date:** 2026-09-25
- **Deciders:** NahuelFernandezGalli
- **Supersedes:** [ADR-0026](0026-authenticate-with-a-development-scheme-until-auth0.md)

## Context

Phase 4 built the whole request pipeline (local user, user-scoped transaction, Row-Level Security)
behind a development scheme that trusted a header and refused to run outside Development. Phase 5
must accept real identities from Auth0 ([ADR-0003](0003-use-auth0-for-authentication.md)), run in
any environment, and keep a test suite that proves token validation without a network or a real
tenant.

## Decision

We authenticate every request with ASP.NET Core's JWT bearer handler, configured for the Auth0
tenant in `Auth0:Domain` and the API identifier in `Auth0:Audience`:

- Only RS256 tokens are accepted, signed by a key of the tenant's JWKS, issued by
  `https://<domain>/`, for the configured audience, within their lifetime.
- Claim names are kept as issued (`MapInboundClaims = false`), so the subject stays `sub`, the
  claim the transaction filter maps to the local user.
- A token without a subject, or with one longer than 255 characters, is rejected as
  unauthenticated.
- Both settings are required in every environment and validated when the host starts.

The development scheme and the `X-Dev-User` header are removed. Tests host the API with a stand-in
tenant: a signing key generated per test run and its OpenID configuration, handed to the handler
as static metadata, so the production validation code runs unchanged.

## Alternatives considered

- **Keep the development scheme next to JWT bearer in Development** — two ways in, tests that
  never exercise real validation, and a header that trusts anyone one misconfiguration away from
  production.
- **Test against a real Auth0 tenant** — needs the network and secrets in CI, and makes the suite
  slow and flaky.
- **Validate tokens by hand with `JsonWebTokenHandler`** — reimplements metadata caching and key
  rotation that the handler already does.

## Consequences

### Positive

- The API runs anywhere it is configured; a missing or malformed setting stops it at startup
  instead of rejecting every request.
- Signature, issuer, audience, lifetime, algorithm and subject are each covered by a test.
- Key rotation at Auth0 needs no change: the handler refreshes the JWKS.

### Negative

- Calling the API by hand needs a real access token, copied from a signed-in web app.
- The first authenticated request after startup downloads the tenant metadata, so the API needs
  outbound HTTPS to Auth0.
- A token stays valid until it expires even after logout; access tokens are kept short (one hour).
