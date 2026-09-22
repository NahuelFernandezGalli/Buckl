# ADR-0021: Use xUnit v3 on Microsoft Testing Platform

- **Status:** Accepted
- **Date:** 2026-09-22
- **Deciders:** NahuelFernandezGalli

## Context

The API solution started in phase 2 on xUnit v2, whose 2.9 line receives no new features. Phase 4
adds four test projects (architecture, application, infrastructure, API) and integration tests
that start Postgres containers. Migrating one small project now is cheaper than migrating five
later, and the maintained line has first-class support for assembly-wide fixtures, which the
database tests need.

## Decision

Every .NET test project uses the `xunit.v3` package and runs on Microsoft Testing Platform (MTP).
`apps/api/global.json` opts the .NET 10 SDK into the MTP mode of `dotnet test`, and test projects
are executables (`OutputType=Exe`, set once in `tests/Directory.Build.props`). Tests are run from
`apps/api`, because the SDK reads `global.json` from the current directory: the pre-push hook runs
`(cd apps/api && dotnet test)` and CI already uses `apps/api` as its working directory.

Tests pass `TestContext.Current.CancellationToken` to every call that accepts a cancellation token,
as the xUnit analyzers require, so an aborted run stops outstanding database and HTTP calls.

## Alternatives considered

- **Stay on xUnit v2** — no work today, a five-project migration later, and no assembly fixtures
  for the shared Postgres container.
- **xUnit v3 on the VSTest bridge** — keeps `Microsoft.NET.Test.Sdk`, but the .NET 10 SDK no
  longer runs MTP-based projects through VSTest; it would pin us to older packages.
- **NUnit or MSTest** — mature alternatives, but every existing test and convention is xUnit.

## Consequences

### Positive

- One maintained test framework for the whole API, with assembly fixtures for the database.
- Fewer packages: `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio` and `coverlet.collector`
  are gone.

### Negative

- `dotnet test apps/api` from the repository root no longer works; the documented command is
  `cd apps/api && dotnet test`.
- Code coverage needs an MTP extension if it is ever wanted (phase 10).
