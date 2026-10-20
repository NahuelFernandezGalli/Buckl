import { describe, expect, it } from 'vitest'
import { ApiError, NetworkError } from '../data/api/api-errors'
import { PhotoUploadError } from '../data/photos/photo-uploader'
import { PhotoUnreadableError } from '../data/photos/prepare-photo'
import { ArchivedGarmentIsReadOnlyError } from '../domain/errors'
import { SessionExpiredError } from '../session/session'
import { describeError } from './describe-error'

const fallback = 'Could not save the garment.'

describe('describeError', () => {
  it.each([
    [new SessionExpiredError(), 'Your session expired. Log in again to keep going.'],
    [
      new NetworkError(new TypeError('Failed to fetch')),
      'Could not reach Buckl. Check your connection and try again.',
    ],
    [
      new PhotoUploadError(new TypeError()),
      'The photo could not be uploaded. Check your connection and try again.',
    ],
    [new PhotoUnreadableError(new Error()), 'This photo could not be read. Try another one.'],
    [new ApiError(400, 'photo.too_large'), 'This photo is too large. Try another one.'],
    [
      new ApiError(400, 'photo.unsupported_type'),
      'This photo format is not supported. Try a JPEG or PNG photo.',
    ],
    [
      new ApiError(400, 'photo.upload_not_found'),
      'The photo did not finish uploading. Please try again.',
    ],
    [
      new ApiError(409, 'garment.archived_read_only'),
      'This garment is archived. Restore it to edit it.',
    ],
    [new ArchivedGarmentIsReadOnlyError('g-1'), 'This garment is archived. Restore it to edit it.'],
    [new ApiError(503, 'server.error'), 'Something went wrong on our side. Please try again.'],
  ])('explains %o', (error, message) => {
    expect(describeError(error, fallback)).toBe(message)
  })

  it.each([
    new ApiError(400, 'money.negative_amount'),
    new ApiError(422, 'request.rejected'),
    new Error('Garment g-1 was not found.'),
    'not even an error',
  ])('falls back for anything it has no words for: %o', (error) => {
    expect(describeError(error, fallback)).toBe(fallback)
  })
})
