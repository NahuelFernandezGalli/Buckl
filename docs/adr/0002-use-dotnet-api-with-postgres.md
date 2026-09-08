# ADR-0002: Use a .NET API with Postgres

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

Buckl needs a backend that owns the domain rules, talks to a relational database with Row-Level
Security, signs storage URLs and validates tokens. The project must show clean layered
architecture, SOLID and TDD, and it must run for free.

## Decision

We build our own API in .NET (ASP.NET Core with EF Core) on top of Postgres, organized in four
projects: `Buckl.Domain`, `Buckl.Application`, `Buckl.Infrastructure` and `Buckl.Api`, with
dependencies pointing inward. Postgres hosting is decided in
[ADR-0004](0004-use-neon-for-postgres-hosting.md).

## Alternatives considered

- **Supabase only (Postgres plus an auto-generated API, no custom backend)** — fastest to start
  and has RLS built in, but it leaves no place for domain logic, use cases or layered
  architecture, which is the point of the portfolio.
- **Node.js with NestJS** — shares the language with the front end, but the author wants to
  demonstrate .NET, and NestJS's structure reimplements what .NET offers natively.
- **Firebase** — a document database with vendor-specific rules; a poor fit for relational data,
  RLS-style isolation and a layered domain.

## Consequences

### Positive

- An explicit domain layer written with TDD, independent from web and database concerns.
- EF Core migrations version the schema, and Postgres RLS gives database-level isolation.
- Strong typing and analyzers with warnings treated as errors.

### Negative

- Two toolchains (npm and .NET) in one repository, so CI needs a separate job.
- Free hosting for a .NET container is scarcer than for static sites; the host is chosen in
  phase 9.
