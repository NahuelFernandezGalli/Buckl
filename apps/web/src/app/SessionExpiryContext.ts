import { createContext } from 'react'

export interface SessionExpiry {
  expired: boolean
}

/**
 * Whether the API has refused the session (ADR-0030: the SDK keeps answering "signed in" from the
 * cached profile after the refresh token dies). Set by `ApiRepositoriesProvider`, read by
 * `SessionExpiredNotice`. Outside the provider, never expired.
 */
export const SessionExpiryContext = createContext<SessionExpiry>({ expired: false })
