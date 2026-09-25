import { Auth0Provider, useAuth0, type AppState } from '@auth0/auth0-react'
import { useMemo, type ReactNode } from 'react'
import type { Auth0Settings } from '../app/config'
import { safeReturnTo } from './return-to'
import { SessionExpiredError, type Session, type SessionState } from './session'
import { SessionContext } from './SessionContext'

export interface Auth0SessionProviderProps {
  settings: Auth0Settings
  /** Called once Auth0 has sent the user back signed in, with the in-app path to show. */
  onSignedIn: (returnTo: string) => void
  children: ReactNode
}

/** Errors of `getAccessTokenSilently` that mean "log in again", not "something broke". */
const expiredSessionErrors = new Set([
  'login_required',
  'consent_required',
  'invalid_grant',
  'missing_refresh_token',
])

/**
 * The Auth0 adapter of the `Session` port (ADR-0029). Tokens are cached in local storage and
 * renewed with rotating refresh tokens, so an installed app survives a reload without relying on
 * third-party cookies (ADR-0030).
 */
export function Auth0SessionProvider({
  settings,
  onSignedIn,
  children,
}: Auth0SessionProviderProps) {
  const origin = window.location.origin
  return (
    <Auth0Provider
      domain={settings.domain}
      clientId={settings.clientId}
      authorizationParams={{ redirect_uri: `${origin}/callback`, audience: settings.audience }}
      cacheLocation="localstorage"
      useRefreshTokens
      useRefreshTokensFallback={false}
      onRedirectCallback={(appState?: AppState) => onSignedIn(safeReturnTo(appState?.returnTo))}
    >
      <Auth0Session>{children}</Auth0Session>
    </Auth0Provider>
  )
}

function Auth0Session({ children }: { children: ReactNode }) {
  const {
    isLoading,
    isAuthenticated,
    user,
    error,
    loginWithRedirect,
    logout,
    getAccessTokenSilently,
  } = useAuth0()

  const session = useMemo<Session>(() => {
    let state: SessionState
    if (isLoading) state = { status: 'loading' }
    else if (isAuthenticated) {
      state = {
        status: 'signedIn',
        user: { displayName: user?.name ?? user?.email ?? 'Signed in' },
      }
    } else if (error) {
      state = {
        status: 'signedOut',
        error: { message: 'We could not sign you in. Please try again.' },
      }
    } else state = { status: 'signedOut' }

    return {
      state,
      signIn: (returnTo) => loginWithRedirect({ appState: { returnTo: safeReturnTo(returnTo) } }),
      signOut: () => logout({ logoutParams: { returnTo: `${window.location.origin}/welcome` } }),
      getAccessToken: async () => {
        let token: string | undefined
        try {
          token = await getAccessTokenSilently()
        } catch (cause) {
          if (isExpiredSession(cause)) throw new SessionExpiredError()
          throw cause
        }
        if (!token) throw new SessionExpiredError()
        return token
      },
    }
  }, [isLoading, isAuthenticated, user, error, loginWithRedirect, logout, getAccessTokenSilently])

  return <SessionContext.Provider value={session}>{children}</SessionContext.Provider>
}

function isExpiredSession(cause: unknown): boolean {
  return (
    typeof cause === 'object' &&
    cause !== null &&
    'error' in cause &&
    typeof cause.error === 'string' &&
    expiredSessionErrors.has(cause.error)
  )
}
