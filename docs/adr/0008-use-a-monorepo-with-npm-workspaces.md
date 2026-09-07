# ADR-0008: Use a monorepo with npm workspaces

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

Buckl has a React front end and a .NET API developed by one person, with documentation that must
stay in sync with both. Cross-cutting changes, such as adding a field to Garment, touch both
sides.

## Decision

One repository, `Buckl`, with npm workspaces at the root. `apps/web` is an npm workspace. The .NET
solution lives in `apps/api` but is not an npm workspace: it is built and tested by its own CI
job. Root scripts (`lint`, `typecheck`, `test`, `build`, `format`) orchestrate the workspaces, and
hooks and formatting are configured once at the root.

## Alternatives considered

- **Two repositories** — independent pipelines, but every cross-cutting change becomes two pull
  requests and the documentation drifts.
- **Nx or Turborepo** — a task graph and caching we do not need with one web workspace and one
  .NET solution.

## Consequences

### Positive

- One pull request per feature, so docs and ADRs travel with the code that motivates them.
- A single place for hooks, Prettier, commitlint and Dependabot.

### Negative

- CI runs the web job on API-only or docs-only changes. Acceptable at this size; path filters can
  be added later.
- Mixed toolchains in one root, so contributors need both Node and the .NET SDK.
