# ADR-0031: Store photos in Backblaze B2

- **Status:** Accepted
- **Date:** 2026-09-29
- **Deciders:** NahuelFernandezGalli
- **Supersedes:** [ADR-0005](0005-use-cloudflare-r2-for-photo-storage.md)

## Context

ADR-0005 chose Cloudflare R2 for garment photos: free at portfolio scale, S3-compatible, with
presigned URLs. Enabling R2 requires a payment method on file, even to stay within the free tier,
and Buckl is meant to run without one. Everything else ADR-0005 asked of photo storage still
holds: private objects, presigned URLs only, keys under `users/<userId>/`, and no credentials in
the browser.

## Decision

We store garment photos in Backblaze B2, through its S3-compatible API.

- One private bucket per environment (`buckl-photos-dev` for development), encrypted at rest
  (SSE-B2), reached at its S3 endpoint `https://s3.<region>.backblazeb2.com` and signed for that
  region.
- The API uses an application key with read and write access to that bucket only.
- CORS allows only `s3_put` from the web app's origins; the rule is set with the B2 CLI, since the
  web console only offers generic rules.
- The free tier covers 10 GB of storage, 1 GB of downloads and 2,500 class B and C transactions a
  day, with no payment method on file; past a cap, requests fail until the next day instead of
  being charged.

## Alternatives considered

- **Keep Cloudflare R2** — no egress fees and a generous free tier, but it needs a payment method.
- **Supabase Storage** — free without a payment method, but 1 GB, a free project pauses after a
  week without use, and its signed uploads are not the S3 API.
- **Amazon S3, Azure Blob Storage, Google Cloud Storage, Firebase Storage** — all need a payment
  method, even for their free tiers.
- **Postgres `bytea` in Neon** — rejected in ADR-0005 for the same reasons as then.

## Consequences

### Positive

- No payment method on any account Buckl depends on.
- The code only depends on the S3 API: going back to R2, or to any S3-compatible store, is a
  configuration change.

### Negative

- Daily caps on downloads and transactions; reaching one makes photos fail to load until
  00:00 GMT.
- Presigned URLs live on B2's S3 endpoint; the static host's content security policy (phase 9)
  has to allow it.
- Lifecycle rules act in whole days: a staging object is hidden after one day and deleted the day
  after.

## References

- https://www.backblaze.com/cloud-storage/pricing
- https://www.backblaze.com/docs/cloud-storage-data-caps-and-alerts
