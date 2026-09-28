# Privacy and personal data

What Buckl stores about a person, where, why, and how it is removed. This document is updated in
the same pull request as any change that adds, moves or removes personal data.

## Principles

- Store the minimum: Buckl keeps what the wardrobe needs and nothing for analytics or profiling.
- Isolation by default: every user-scoped row is protected by Row-Level Security, described in
  [Authentication and security](architecture/auth-and-security.md).
- No secrets or personal data in logs, URLs or the front-end bundle.
- Deletion is complete: removing an account removes the data in every system listed below.

## Data inventory

| Data                                                                    | Where                                    | Why                                                                                                    | Retention                                                                                                                                                 |
| ----------------------------------------------------------------------- | ---------------------------------------- | ------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Email, name, login credentials, social identity                         | Auth0                                    | Sign-up and login                                                                                      | Until account deletion                                                                                                                                    |
| Auth0 subject (`sub`), internal user id and creation time               | Neon (`users`)                           | Link garments and outfits to their owner; nothing else from the token (name, email, picture) is stored | Until account deletion                                                                                                                                    |
| Garment classification, notes, status                                   | Neon (`garments`)                        | The wardrobe itself                                                                                    | Until the user archives or deletes the garment                                                                                                            |
| Purchase price, currency and date                                       | Neon (`garments`)                        | Show what was paid; imports fill it in                                                                 | Until the garment is deleted                                                                                                                              |
| Garment photos                                                          | Cloudflare R2                            | Show the garment                                                                                       | Until the garment is deleted                                                                                                                              |
| Product catalog data (brand, name, image URL, source URL)               | Neon (`products`)                        | Shared, impersonal reference                                                                           | Indefinite; not linked to a person                                                                                                                        |
| Outfits and wear log _(phase 8)_                                        | Neon                                     | Suggestions and history                                                                                | Until account deletion                                                                                                                                    |
| Product URLs shared for import _(phase 7)_                              | Neon (`products.source_url`)             | Re-open the product page                                                                               | Indefinite; the URL is public                                                                                                                             |
| Purchase confirmation emails _(phase 7)_                                | Not stored                               | Parsed in memory to extract items; the raw email is discarded                                          | None                                                                                                                                                      |
| Access, refresh and ID tokens (the ID token carries the name and email) | Browser local storage, on Buckl's origin | Call the API and keep the session across reloads                                                       | Until logout. After 15 days unused (30 at most) they no longer sign anyone in, but the cached profile (name and email) stays in the browser until logout. |
| Server logs                                                             | API host                                 | Operate the service                                                                                    | Short; no personal data, no tokens, correlation ids only                                                                                                  |

Buckl does not use analytics, advertising identifiers or third-party tracking scripts.

## Third parties (processors)

| Service       | Data                           | Location and notes                             |
| ------------- | ------------------------------ | ---------------------------------------------- |
| Auth0         | Identity and login             | Managed identity provider                      |
| Neon          | Database rows                  | Managed Postgres; region chosen at creation    |
| Cloudflare R2 | Photos                         | Object storage; private bucket, presigned URLs |
| Static host   | None; serves public assets     | Cloudflare Pages or equivalent                 |
| API host      | Processes requests, keeps logs | Chosen in phase 9                              |

## Access

- A user can see and edit all of their data in the app.
- The maintainer can read production data only through database administration for support, and
  never copies it elsewhere.

## Deletion

Account deletion, performed manually by the maintainer on request in v1:

1. Delete the user's garments, outfits and wear logs in Neon, cascading from `users`.
2. Delete every object under `users/<userId>/` in Cloudflare R2.
3. Delete the user in Auth0.

Products stay because they carry no personal data. A self-service "delete my account" action is a
candidate for a later phase.

## Photos

Photos may show a person, a room or a location. They are private to the owner, served only through
short-lived signed URLs, and never used for any purpose other than showing the garment to its
owner. Client-side compression in phase 6 reduces resolution before upload.

## Changes

| Date       | Change                                                                                        |
| ---------- | --------------------------------------------------------------------------------------------- |
| 2026-09-07 | Initial version, before any data exists.                                                      |
| 2026-09-25 | Phase 5: the API stores only the Auth0 subject of each user, from the validated access token. |
| 2026-09-25 | Phase 5: the web app keeps the Auth0 tokens in the browser's local storage.                   |
| 2026-09-25 | Phase 5: clarified that the cached profile stays in the browser until logout.                 |
