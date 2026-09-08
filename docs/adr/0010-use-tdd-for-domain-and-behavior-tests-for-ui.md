# ADR-0010: Use TDD for domain and services, behavior tests for UI components

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

The domain (garments, products, outfit rules) is where Buckl's correctness lives, and it has no UI
or database to hide behind. UI screens, on the other hand, take shape by iteration, and tests
written before the first render tend to describe layouts rather than behavior.

## Decision

- Domain and application services in .NET: strict test-driven development with xUnit. A failing
  test comes first, then the minimal code, then refactoring.
- Infrastructure adapters: integration tests against real dependencies where feasible, such as
  Postgres in a container, with test doubles otherwise.
- UI components in React: behavior tests with Testing Library and Vitest, written once a screen
  takes shape, asserting what the user sees and does rather than implementation details.
- A pull request includes the tests of what it changes, and CI runs them all.

## Alternatives considered

- **TDD everywhere, including UI** — slows screen design and produces brittle tests.
- **Tests after the fact everywhere** — loses the design pressure TDD puts on the domain.

## Consequences

### Positive

- The domain grows from executable examples, and the suite catches regressions.
- UI tests survive visual refactors.

### Negative

- A discipline cost: the domain cannot be rushed, and test doubles for ports must be maintained.
