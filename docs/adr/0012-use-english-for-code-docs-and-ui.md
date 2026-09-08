# ADR-0012: Use English for code, documentation and UI

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

The author's working language is Spanish, but the project is a portfolio meant to be read by
reviewers anywhere, and the libraries, frameworks and error messages it depends on are in English.

## Decision

Everything committed to the repository is in English: identifiers, comments, documentation, commit
messages, pull requests, UI copy and test names. Planning notes kept outside the repository may be
in any language.

## Alternatives considered

- **A Spanish UI with English code** — splits the vocabulary in two and makes the glossary
  ambiguous.

## Consequences

### Positive

- One vocabulary shared by the glossary, the code and the screens.

### Negative

- UI copy needs care for a Spanish-speaking first user, and localization becomes a possible later
  feature.
