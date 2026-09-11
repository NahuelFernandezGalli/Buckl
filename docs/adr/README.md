# Architecture decision records

An ADR captures one decision, the context that forced it, the alternatives and the consequences.
ADRs are immutable once accepted: to change a decision, write a new ADR that supersedes the old
one and update the old one's status.

## How to write one

1. Copy [template.md](template.md) to `NNNN-short-title.md`, using the next free number (four
   digits, zero padded) and a kebab-case title.
2. Fill every section. Keep it under about 80 lines.
3. Add a row to the log below in the same pull request.

## Log

| ADR                                                                 | Title                                                                   | Status                                                       | Date       |
| ------------------------------------------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------ | ---------- |
| [0001](0001-use-react-for-the-web-app.md)                           | Use React for the web app                                               | Accepted                                                     | 2026-09-07 |
| [0002](0002-use-dotnet-api-with-postgres.md)                        | Use a .NET API with Postgres                                            | Accepted                                                     | 2026-09-07 |
| [0003](0003-use-auth0-for-authentication.md)                        | Use Auth0 for authentication                                            | Accepted                                                     | 2026-09-07 |
| [0004](0004-use-neon-for-postgres-hosting.md)                       | Use Neon for Postgres hosting                                           | Accepted                                                     | 2026-09-07 |
| [0005](0005-use-cloudflare-r2-for-photo-storage.md)                 | Use Cloudflare R2 for photo storage                                     | Accepted                                                     | 2026-09-07 |
| [0006](0006-deploy-web-as-static-site.md)                           | Deploy the web app as a static site                                     | Accepted                                                     | 2026-09-07 |
| [0007](0007-enforce-tenant-isolation-with-rls.md)                   | Enforce tenant isolation with Row-Level Security via a session variable | Accepted                                                     | 2026-09-07 |
| [0008](0008-use-a-monorepo-with-npm-workspaces.md)                  | Use a monorepo with npm workspaces                                      | Accepted                                                     | 2026-09-07 |
| [0009](0009-use-conventional-commits-and-branch-naming.md)          | Use Conventional Commits and typed branch names                         | Accepted                                                     | 2026-09-07 |
| [0010](0010-use-tdd-for-domain-and-behavior-tests-for-ui.md)        | Use TDD for domain and services, behavior tests for UI components       | Superseded by [0013](0013-use-bdd-for-front-end-features.md) | 2026-09-07 |
| [0011](0011-gate-merges-with-local-hooks-and-ci.md)                 | Gate merges with local hooks and required CI checks                     | Accepted                                                     | 2026-09-07 |
| [0012](0012-use-english-for-code-docs-and-ui.md)                    | Use English for code, documentation and UI                              | Accepted                                                     | 2026-09-07 |
| [0013](0013-use-bdd-for-front-end-features.md)                      | Use TDD for the domain and BDD for front-end features                   | Accepted                                                     | 2026-09-08 |
| [0014](0014-target-the-current-dotnet-lts.md)                       | Target the current .NET LTS release and pin the SDK                     | Accepted                                                     | 2026-09-10 |
| [0015](0015-signal-domain-rule-violations-with-typed-exceptions.md) | Signal domain rule violations with typed exceptions                     | Accepted                                                     | 2026-09-10 |
