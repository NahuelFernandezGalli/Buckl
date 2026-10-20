# ADR-0034: Mirror the API contract by hand in the web app

- **Status:** Accepted
- **Date:** 2026-09-29
- **Deciders:** NahuelFernandezGalli

## Context

ADR-0019 wrote the web app's domain types by hand, as a mirror of the API, and left for phase 6
whether to generate them from the API's OpenAPI document instead. In phase 6 the HTTP
repositories start depending on the real contract. The contract is small (two resources, one
upload ticket), and the OpenAPI document still describes partial updates (`FieldUpdate<T>`)
incorrectly.

## Decision

We keep the web app's types hand-written and pin the contract with tests on both sides: the API's
endpoint tests assert every field of the JSON it returns, and the web app's HTTP repositories are
tested with bodies in the same shape (`GarmentMother`, `fakeFetch`). A change to the contract
changes both test suites in the same pull request.

## Alternatives considered

- **Generate types with openapi-typescript in CI** — catches drift automatically, but needs a
  correct OpenAPI schema for partial updates first, and adds a build step for two resources.
- **A shared JSON fixture read by both test suites** — one source of truth, but couples the two
  apps' test projects through the file system.

## Consequences

### Positive

- No generator, no extra build step; the types read like the rest of the domain code.

### Negative

- A field renamed on one side only fails when a test on the other side exercises it; reviews of
  contract changes must look at both apps.
- Revisit when the contract grows (outfits, imports) or once the OpenAPI schema is exact.
