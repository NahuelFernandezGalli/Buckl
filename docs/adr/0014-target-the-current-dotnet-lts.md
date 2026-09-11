# ADR-0014: Target the current .NET LTS release and pin the SDK

- **Status:** Accepted
- **Date:** 2026-09-10
- **Deciders:** NahuelFernandezGalli

## Context

The API solution starts in phase 2 with the domain project. .NET 8 (LTS) is installed on the
development machine but leaves support on 2026-11-10; .NET 10 is the current LTS, supported until
2028-11. A portfolio project should not start on a runtime two months from end of life, and the
version must be the same on every machine and in CI.

## Decision

Every project in `apps/api` targets `net10.0`. `apps/api/global.json` pins the SDK to the version
installed today with `rollForward: latestFeature`, so a newer 10.0 feature band works and a newer
major is never picked up by accident. The target framework is declared once, in
`Directory.Build.props`. Moving to the next LTS is a deliberate change: a new ADR, a bump in both
files, and a green CI run.

The solution uses the XML solution format (`Buckl.slnx`) that the .NET 10 SDK generates by
default. Commands take the directory rather than the file name (`dotnet build apps/api`), so the
format is an implementation detail.

## Alternatives considered

- **.NET 8** — already installed, but end of life in two months; it would force a migration in
  phase 10.
- **Latest STS release** — shorter support windows and yearly upgrades for no benefit here.
- **No `global.json`** — the machine's newest SDK would win, and CI and local builds could differ.

## Consequences

### Positive

- Three more years of support and access to current language and runtime features.
- Local, CI and future container builds share one SDK version.

### Negative

- The SDK must be installed explicitly on each machine; the root README documents it.
- Free API hosts (phase 9) must offer the .NET 10 runtime or run a container.
