import { describe, expect, it } from 'vitest'
import { fakeFetch, jsonResponse, problemResponse, testApiClient } from '../../test/fake-fetch'
import { ApiError } from '../api/api-errors'
import { createPhotoUploader, PhotoUploadError } from './photo-uploader'

const STORAGE_URL = 'https://storage.test/uploads/alice/upload-1?X-Amz-Signature=abc'

const ticket = {
  uploadId: 'upload-1',
  url: STORAGE_URL,
  method: 'PUT' as const,
  headers: { 'Content-Type': 'image/jpeg' },
  expiresAt: '2026-09-29T12:10:00+00:00',
}

function setup(
  storageAnswer: () => Response | Promise<Response> = () => new Response(null, { status: 200 }),
) {
  const http = fakeFetch()
  http.on('POST /photos/uploads', () => jsonResponse(ticket))
  http.on(`PUT ${STORAGE_URL}`, storageAnswer)
  const prepared = new Blob(['small jpeg'], { type: 'image/jpeg' })
  const upload = createPhotoUploader({
    api: testApiClient(http),
    prepare: async () => prepared,
    fetch: http.fetch,
  })
  return { http, prepared, upload }
}

const picked = () => new File(['a big original photo'], 'IMG_0001.jpg', { type: 'image/jpeg' })

describe('createPhotoUploader', () => {
  it('uploads the prepared photo, not the original, and returns its upload id', async () => {
    const { http, prepared, upload } = setup()

    await expect(upload(picked())).resolves.toBe('upload-1')

    const [ticketRequest, put] = http.requests
    expect(ticketRequest.body).toEqual({ contentType: 'image/jpeg', size: prepared.size })
    expect(put.method).toBe('PUT')
    expect(put.body).toBe(prepared)
  })

  it('sends storage exactly the ticket headers and never the access token', async () => {
    const { http, upload } = setup()

    await upload(picked())

    const put = http.requests[1]
    expect(put.headers.get('Content-Type')).toBe('image/jpeg')
    expect(put.headers.has('Authorization')).toBe(false)
  })

  it.each([
    [
      'refuses the upload',
      () => new Response('<Error>SignatureDoesNotMatch</Error>', { status: 403 }),
    ],
    [
      'cannot be reached',
      () => {
        throw new TypeError('Failed to fetch')
      },
    ],
  ])('reports a failed upload when storage %s', async (_case, storageAnswer) => {
    const { upload } = setup(storageAnswer)

    await expect(upload(picked())).rejects.toBeInstanceOf(PhotoUploadError)
  })

  it('does not upload a photo the API refuses to sign', async () => {
    const http = fakeFetch()
    http.on('POST /photos/uploads', () => problemResponse(400, 'photo.too_large'))
    const upload = createPhotoUploader({
      api: testApiClient(http),
      prepare: async (file) => file,
      fetch: http.fetch,
    })

    await expect(upload(picked())).rejects.toMatchObject({ code: 'photo.too_large' })
    await expect(upload(picked())).rejects.toBeInstanceOf(ApiError)
    expect(http.requests.every((request) => request.method === 'POST')).toBe(true)
  })

  it('does not ask for a ticket when the photo cannot be prepared', async () => {
    const http = fakeFetch()
    const upload = createPhotoUploader({
      api: testApiClient(http),
      prepare: async () => {
        throw new Error('unreadable')
      },
      fetch: http.fetch,
    })

    await expect(upload(picked())).rejects.toThrow('unreadable')
    expect(http.requests).toEqual([])
  })
})
