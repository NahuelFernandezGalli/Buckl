# ADR-0016: Store enumerations as text with check constraints

- **Status:** Accepted
- **Date:** 2026-09-10
- **Deciders:** NahuelFernandezGalli

## Context

The domain has small closed sets (`Category`, `Color`, `ImportSource`, `GarmentStatus`) modeled as
C# enums. Postgres can store them as a native enum type, as integers, or as text. Phase 8 will
probably add values, such as finer categories or occasions, so the cost of changing a set is what
matters most.

## Decision

Enumerations are persisted as lower-case `text` columns with a `check (... in (...))` constraint
per column. EF Core converts between the enum name and the lower-case string. Adding a value is
one migration that replaces the constraint, and the integration tests of phase 4 keep the enum and
the constraint list in step.

## Alternatives considered

- **A native Postgres enum type** — compact and self-documenting, but adding values is awkward
  inside a transaction, removing or renaming one needs a type rebuild, and EF Core needs extra
  mapping. Not worth it for six-value sets.
- **Integer columns** — the smallest option, but rows become unreadable in SQL and the data breaks
  silently if an enum is ever reordered.

## Consequences

### Positive

- Readable rows and constraints in any SQL client, and a trivial migration for a new value.
- The database rejects values the domain does not know, even from an ad-hoc write.

### Negative

- Two lists to keep aligned, the enum and the constraint, covered by a test in phase 4.
- Slightly more storage than integers, which is irrelevant at this scale.
