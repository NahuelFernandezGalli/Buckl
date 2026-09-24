# ADR-0023: Persist aggregates through dedicated persistence records

- **Status:** Accepted
- **Date:** 2026-09-22
- **Deciders:** NahuelFernandezGalli

## Context

EF Core can map domain aggregates directly, but only if the domain bends: parameterless
constructors on entities and value objects, settable backing fields, and complex types for
`Classification` and an optional `PurchaseInfo` that nests `Money`. `PhotoKey` also carries its
owner, which the table stores once, in `user_id`. The domain of phase 2 was written without any of
those concessions, and keeping it free of persistence concerns is one of the goals of the project.

## Decision

EF Core maps plain persistence records (`GarmentRecord`, `ProductRecord`, `UserRecord`) that have
one property per column. Repositories in `Buckl.Infrastructure` convert records to aggregates with
the domain's `Rehydrate` factories and aggregates to records with explicit mappers. The domain's
only concession is those factories, which re-check invariants that span fields.

Enumerations are stored as lower-case text (ADR-0016) by `EnumText`, and the lists inside the
check constraints are generated from the enums, so a new enum member changes the model and fails
the "no pending model changes" test until a migration updates the constraint. Column names are
snake case through `EFCore.NamingConventions`.

## Alternatives considered

- **Map the aggregates directly** — less code, but parameterless constructors and complex-type
  configuration would leak EF Core into the domain, and nullable nested complex types are the part
  of EF Core with the most caveats.
- **Dapper with hand-written SQL** — full control, but migrations, change tracking and query
  composition for the wardrobe filters would all be ours to maintain.

## Consequences

### Positive

- The domain stays free of persistence concerns; the architecture tests enforce it.
- Queries such as the wardrobe filters are written against flat columns.

### Negative

- Two representations of each aggregate and a mapper per aggregate to keep in step, covered by
  round-trip tests.
- Updates copy fields from the aggregate onto the tracked record instead of relying on EF Core to
  track the aggregate itself.
