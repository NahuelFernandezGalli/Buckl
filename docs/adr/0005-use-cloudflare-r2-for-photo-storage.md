# ADR-0005: Use Cloudflare R2 for photo storage

- **Status:** Accepted
- **Date:** 2026-09-07
- **Deciders:** NahuelFernandezGalli

## Context

Garment photos are the largest data Buckl stores and must never go into the database. Storage must
be free at portfolio scale, S3-compatible so standard SDKs and presigned URLs work, and it must
not require credentials in the browser.

## Decision

We store photos in Cloudflare R2, accessed only through presigned URLs issued by the API. Keys are
namespaced by user (`users/<userId>/...`). The web app uploads directly to R2 with a presigned PUT
and reads with short-lived presigned GET URLs.

## Alternatives considered

- **AWS S3** — the reference implementation, but its free tier is time-limited and egress costs
  money.
- **Storing photos in Postgres as `bytea`** — simple, but it bloats the database and its free
  tier.
- **Supabase Storage** — tied to a platform we are not using.

## Consequences

### Positive

- No egress fees, and an S3-compatible API means the storage adapter stays portable.
- The browser never sees storage credentials; the API controls key, size and content type.

### Negative

- One more service to configure and to include in account deletion.
- Presigned URLs expire, so the front end must handle refreshing them.
