# ADR-0011: Gate merges with local hooks and required CI checks

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

With one developer there is no reviewer to catch a broken build or a leaked secret. The repository
itself must refuse bad changes, both early on the developer's machine and definitively on the
server.

## Decision

- Local hooks through husky: `pre-commit` runs lint-staged (ESLint fix and Prettier on staged
  files), `commit-msg` runs commitlint, and `pre-push` runs typecheck, tests and build.
- GitHub Actions on every pull request: `CI / Web` (lint, typecheck, test, build),
  `PR title / Conventional Commits`, `Security / Secret scan` with gitleaks, and
  `Security / Dependency audit` with `npm audit --audit-level=high`. Security also runs weekly,
  and Dependabot opens update pull requests.
- `main` is protected by a ruleset: changes only through pull requests, the four checks above
  required, and no bypass list. Zero approvals are required because there is one maintainer.
- When the .NET solution exists, its build-and-test job becomes a fifth required check.

## Alternatives considered

- **Hooks only** — bypassable with `--no-verify`, and nothing enforces the rules on the server.
- **CI only** — catches problems late, after a push.

## Consequences

### Positive

- A red check blocks the merge, and secrets or vulnerable dependencies are caught automatically.
- The developer gets the same feedback locally before pushing.

### Negative

- `pre-push` takes about a minute even for documentation-only changes.
- Dependabot pull requests need attention regularly.
