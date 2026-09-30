import { ApiError, NetworkError } from '../data/api/api-errors'
import { SessionExpiredError } from '../session/session'

/**
 * Words a person can act on, by the stable code the API, the domain mirror and the photo pipeline
 * attach to their errors. Validation codes are missing on purpose: the form checks the same rules
 * before sending, so they fall back to the screen's own message.
 */
const byCode: Record<string, string> = {
  'garment.archived_read_only': 'This garment is archived. Restore it to edit it.',
  'garment.already_archived': 'This garment is already archived.',
  'garment.not_archived': 'This garment is not archived.',
  'garment.not_found': 'This garment no longer exists.',
  'photo.too_large': 'This photo is too large. Try another one.',
  'photo.unsupported_type': 'This photo format is not supported. Try a JPEG or PNG photo.',
  'photo.empty': 'This photo is empty. Try another one.',
  'photo.upload_not_found': 'The photo did not finish uploading. Please try again.',
  'photo.upload_failed': 'The photo could not be uploaded. Check your connection and try again.',
  'photo.unreadable': 'This photo could not be read. Try another one.',
}

/** What to tell the user about a failure; `fallback` when there is nothing more specific. */
export function describeError(error: unknown, fallback: string): string {
  if (error instanceof SessionExpiredError) {
    return 'Your session expired. Log in again to keep going.'
  }
  if (error instanceof NetworkError) {
    return 'Could not reach Buckl. Check your connection and try again.'
  }
  const code = codeOf(error)
  if (code !== null && Object.hasOwn(byCode, code)) return byCode[code]
  if (error instanceof ApiError && error.status >= 500) {
    return 'Something went wrong on our side. Please try again.'
  }
  return fallback
}

function codeOf(error: unknown): string | null {
  return typeof error === 'object' &&
    error !== null &&
    'code' in error &&
    typeof error.code === 'string'
    ? error.code
    : null
}
