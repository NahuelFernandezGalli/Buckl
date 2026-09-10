# ADR-0015: Signal domain rule violations with typed exceptions

- **Status:** Accepted
- **Date:** 2026-09-10
- **Deciders:** NahuelFernandezGalli

## Context

Value objects and aggregates reject invalid input and illegal state transitions. The domain needs
one way to report those failures that keeps the rule next to the code that enforces it, is easy to
assert in tests, and can be mapped to HTTP problem details in phase 4 without matching on message
text.

## Decision

Every rule the domain can reject throws an exception derived from `DomainException`, which carries
a stable, machine-readable `Code` (for example `money.negative_amount`,
`garment.already_archived`) and a human message. Invalid input to factories and value objects
throws `DomainValidationException` with the code of the violated rule; illegal state transitions
on aggregates throw a dedicated subtype per rule (for example `GarmentAlreadyArchivedException`).
Factories never return a half-built object. Programming errors, such as a null argument, throw the
usual `ArgumentNullException` and are not domain exceptions.

Codes are constants on the type that raises them (`Money.Errors.NegativeAmount`) and are part of
the public contract: the API maps them to problem details and the web app may key messages on
them. They are listed in [the domain model](../architecture/domain-model.md).

## Alternatives considered

- **Result objects (`Result<T>` with an error list)** — makes failure explicit in signatures, but
  every call site must unwrap, value-object construction becomes verbose, and constructors cannot
  return results. Reconsider for use cases in phase 4 if validation needs to aggregate many errors
  at once.
- **`ArgumentException` and friends** — no stable code, and domain rules get mixed with
  programming errors.
- **One generic `DomainException` carrying a string** — loses the ability to catch a specific
  rule.

## Consequences

### Positive

- One rule, one code, one place; tests assert on `Code`, not on message text.
- Phase 4 maps `DomainValidationException` to `400` and state-rule exceptions to `409`, with the
  code in the body, uniformly.

### Negative

- Exceptions as control flow for expected failures; acceptable because a rejected garment edit is
  rare and not a hot path.
- Aggregating several validation errors in one response is not possible with this mechanism alone;
  the application layer validates request shape before reaching the domain.
