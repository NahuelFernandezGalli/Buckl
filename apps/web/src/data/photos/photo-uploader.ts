import type { ApiClient } from '../api/api-client'
import { requestJson } from '../api/responses'

/** The answer of `POST /photos/uploads` (docs/architecture/api.md). */
export interface PhotoUploadTicket {
  uploadId: string
  url: string
  method: 'PUT'
  headers: Record<string, string>
  expiresAt: string
}

/** Storage did not take the photo: no connection, or the signed URL was refused or expired. */
export class PhotoUploadError extends Error {
  readonly code = 'photo.upload_failed'

  constructor(cause: unknown) {
    super('The photo could not be uploaded to storage.', { cause })
    this.name = 'PhotoUploadError'
  }
}

/** Uploads a picked photo; resolves with the upload id the garment request refers to. */
export type PhotoUploader = (file: Blob) => Promise<string>

export interface PhotoUploaderOptions {
  api: ApiClient
  /** Turns the picked file into what is stored: `preparePhoto` in the app. */
  prepare: (file: Blob) => Promise<Blob>
  fetch?: typeof fetch
}

/**
 * The browser's side of ADR-0032: prepare the photo, ask the API for a ticket, and PUT the photo
 * straight to storage with exactly the ticket's headers. The PUT never goes through `api`: the
 * signed URL is the credential, and the access token must not travel to storage.
 */
export function createPhotoUploader({
  api,
  prepare,
  fetch: put = globalThis.fetch,
}: PhotoUploaderOptions): PhotoUploader {
  return async (file) => {
    const photo = await prepare(file)
    const ticket = await requestJson<PhotoUploadTicket>(api, '/photos/uploads', {
      method: 'POST',
      body: { contentType: photo.type, size: photo.size },
    })

    let response: Response
    try {
      // Called as a plain function: browsers refuse fetch invoked on another object.
      response = await put(ticket.url, {
        method: ticket.method,
        headers: ticket.headers,
        body: photo,
      })
    } catch (cause) {
      // An expired URL is a 403 without CORS headers, which the browser reports like this too.
      throw new PhotoUploadError(cause)
    }
    if (!response.ok) throw new PhotoUploadError(new Error(`Storage answered ${response.status}.`))
    return ticket.uploadId
  }
}
