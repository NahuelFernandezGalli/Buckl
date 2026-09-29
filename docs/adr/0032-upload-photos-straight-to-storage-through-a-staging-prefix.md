# ADR-0032: Upload photos straight to storage through a staging prefix

- **Status:** Accepted
- **Date:** 2026-09-29
- **Deciders:** NahuelFernandezGalli

## Context

ADR-0031 keeps garment photos in Backblaze B2, behind presigned URLs. Phase 6 has to decide how a
photo actually travels: the browser must never hold storage credentials, a garment must never
point at an object the API did not check, an abandoned upload must not stay forever, and a
presigned PUT cannot cap the size of what is uploaded.

## Decision

We upload photos from the browser straight to B2 with a presigned PUT to a staging key, and the
API moves the object under the owner's photos when a garment starts using it.

- `POST /photos/uploads` checks the declared content type and size and signs a PUT, valid for
  ten minutes, to `uploads/<userId>/<uploadId>`, with the content type inside the signature.
- The garment request carries `photo: { uploadId }`. The API derives the staging key from the
  caller, so another user's upload is simply not found; it reads the stored object's real size
  and type, deletes it if it breaks the rules, and otherwise copies it to
  `users/<userId>/garments/<uploadId>.<ext>` and deletes the staging object.
- A lifecycle rule hides everything under `uploads/` after one day, which removes it from the
  S3 API, and deletes it the day after.
- Responses carry `photoUrl`, a presigned GET valid for one hour.
- The adapter is `S3PhotoStorage` over the AWS SDK for .NET, behind `IPhotoStorage`, sending
  checksums only when an operation requires them (not every S3-compatible store accepts the
  SDK's default ones). Its tests run against Floci, an S3 emulator, in Testcontainers.

## Alternatives considered

- **Presigning the final key directly** — simpler, but every abandoned upload stays under
  `users/` with nothing to clean it.
- **Uploading through the API** — the API sees every byte and can cap it, but pays the bandwidth
  and the request size limits of a free host (phase 9).
- **A public bucket for reads** — cacheable and simple, but anyone holding a key sees the photo
  forever.
- **Signing with hand-written SigV4** — no SDK, but our own cryptography to maintain.
- **MinIO in tests** — its images are no longer published, and Testcontainers removed the module.

## Consequences

### Positive

- The browser holds a URL for one object and a few minutes, never a credential.
- Size and type are enforced on the stored object, not on what the browser claims.
- Abandoned uploads disappear on their own.

### Negative

- Attaching a photo costs three storage calls (HEAD, copy, delete) inside the request.
- If the database fails after the copy, the copied object stays until the account is deleted.
- Read URLs change on every response, so the browser cannot cache photos across visits.
- What only B2 does (CORS, rejecting a PUT with another content type) is verified by hand, not by
  the emulator.

## References

- https://www.backblaze.com/docs/cloud-storage-lifecycle-rules
- https://www.backblaze.com/docs/cloud-storage-cross-origin-resource-sharing-rules
