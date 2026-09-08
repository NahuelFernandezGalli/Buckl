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

| ADR                                          | Title                        | Status   | Date       |
| -------------------------------------------- | ---------------------------- | -------- | ---------- |
| [0001](0001-use-react-for-the-web-app.md)    | Use React for the web app    | Accepted | 2026-09-07 |
| [0002](0002-use-dotnet-api-with-postgres.md) | Use a .NET API with Postgres | Accepted | 2026-09-07 |
| [0003](0003-use-auth0-for-authentication.md) | Use Auth0 for authentication | Accepted | 2026-09-07 |
