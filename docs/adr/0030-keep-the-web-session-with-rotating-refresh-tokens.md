# ADR-0030: Keep the web session in local storage with rotating refresh tokens

- **Status:** Accepted
- **Date:** 2026-09-25
- **Deciders:** NahuelFernandezGalli

## Context

Buckl is mostly used as an installed app on a phone, so it is reloaded all the time. The Auth0 SDK
keeps tokens in memory by default and renews them through a hidden iframe that needs Auth0's
cookies, which Safari, Firefox and increasingly Chrome block as third-party cookies. With that
default, an iPhone user would log in on every launch. A custom Auth0 domain on Buckl's own site
would make those cookies first-party, but Buckl has no domain until phase 9.

## Decision

We cache tokens in local storage and renew them with rotating refresh tokens:
`cacheLocation: 'localstorage'`, `useRefreshTokens: true` and `useRefreshTokensFallback: false` in
the SDK. In Auth0, refresh token rotation is on with reuse detection, refresh tokens expire after
15 days without use and 30 days at most, and access tokens after one hour. Logout clears the cache.

## Alternatives considered

- **Memory cache with the iframe** — the SDK default; breaks on Safari and in iOS installed apps.
- **Memory cache with refresh tokens** — the refresh token lives in a web worker and dies with the
  page, so every reload falls back to the iframe or to a new login.
- **Custom domain with the memory cache** — the most secure option; needs a domain. Revisit in
  phase 9.
- **Proof of possession (DPoP)** — binds tokens to the browser; the API would have to validate
  proofs. Revisit in phase 10.
- **A backend for frontend with cookies** — contradicts the static web app
  ([ADR-0006](0006-deploy-web-as-static-site.md)).

## Consequences

### Positive

- Sessions survive reloads and app launches on every browser.
- A stolen refresh token is single use: rotation with reuse detection revokes the whole family when
  it is replayed.

### Negative

- Any script running on Buckl's origin can read the tokens, so a cross-site scripting bug means
  token theft. Buckl loads no third-party scripts, React escapes output, and the static host gets
  a strict content security policy in phase 9.
- Tokens stay in the browser until logout or expiry, including on a shared computer.
