# ADR-0009: Use Conventional Commits and typed branch names

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

Commit history and pull request titles are part of what a portfolio reviewer reads. A fixed format
makes history scannable, lets tooling validate it, and keeps changelogs possible later.

## Decision

- Commit messages and pull request titles follow Conventional Commits:
  `type(optional scope): description`, for example `feat(garment): add photo upload`.
- Allowed types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `style`, `perf` and `ci`.
- Branches are named `type/short-description-in-kebab-case`, for example
  `feat/photo-upload-garment`. They are created from `main` and merged back to `main` by pull
  request.
- commitlint validates commit messages locally through the `commit-msg` hook, and a GitHub Action
  validates pull request titles.

## Alternatives considered

- **Free-form messages** — no tooling and an inconsistent history.
- **Gitmoji** — visual, but not machine-readable in the same way.

## Consequences

### Positive

- History reads as a changelog, and scope names map to the glossary.
- Invalid messages are rejected before they reach the repository.

### Negative

- Small friction on every commit, and squash merges must keep a conventional title.
