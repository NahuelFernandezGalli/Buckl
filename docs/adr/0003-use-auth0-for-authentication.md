# ADR-0003: Use Auth0 for authentication

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

Buckl needs sign-up, login and session handling for a single-page app, plus token validation in an
API. Storing passwords ourselves adds risk and work that does not showcase anything. The provider
must have a free tier suitable for a portfolio app and standard OIDC and JWT support.

## Decision

We use Auth0 as the identity provider. The web app uses the Auth0 SPA SDK (Authorization Code with
PKCE); the API validates Auth0-issued JWTs by issuer and audience. The API keeps a minimal local
`users` row keyed by the token's `sub` claim, as described in
[Authentication and security](../architecture/auth-and-security.md).

## Alternatives considered

- **Clerk** — polished SDKs and UI components, but its React-first components pull UI decisions
  into the provider, and its free tier and .NET support were less compelling for this project.
- **Self-managed ASP.NET Identity** — full control, but we would own password storage, reset flows
  and multi-factor authentication; not worth the risk for a portfolio project.
- **Supabase Auth** — tied to the Supabase platform we are not using.

## Consequences

### Positive

- No passwords in our database, standard OIDC, and social logins available later.
- Token validation in .NET is one middleware plus configuration.

### Negative

- A vendor dependency: user records live in Auth0 and account deletion must cover both systems.
- Free tier limits (monthly active users, available features) must be respected.
