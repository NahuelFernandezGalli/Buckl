/**
 * The port through which screens see who is signed in (ADR-0029). Auth0 lives behind it, in
 * `Auth0SessionProvider`; tests use `fakeSession` instead.
 */
export interface Session {
  state: SessionState
  /** Leaves the app for the Auth0 login page; comes back to `returnTo` once signed in. */
  signIn(returnTo: string): Promise<void>
  /** Ends the Auth0 session and comes back to the welcome screen. */
  signOut(): Promise<void>
  /** An access token for the Buckl API, refreshed silently. Rejects with `SessionExpiredError`
   * when the user has to log in again. */
  getAccessToken(): Promise<string>
}

export type SessionState =
  | { status: 'loading' }
  | { status: 'signedOut'; error?: SignInError }
  | { status: 'signedIn'; user: SessionUser }

export interface SessionUser {
  /** What the header shows: the Auth0 profile name, which is the email for database users. */
  displayName: string
}

/** Why the last sign-in attempt failed, in words a person can act on. */
export interface SignInError {
  message: string
}

export class SessionExpiredError extends Error {
  constructor() {
    super('The session expired. Log in again.')
    this.name = 'SessionExpiredError'
  }
}
