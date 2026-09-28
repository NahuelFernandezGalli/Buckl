import { Auth0Provider, GenericError, useAuth0 } from '@auth0/auth0-react'
import { cleanup, render, screen } from '@testing-library/react'
import { useEffect, type ComponentProps } from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { Auth0SessionProvider } from './Auth0SessionProvider'
import { SessionExpiredError, type Session } from './session'
import { useSession } from './useSession'

vi.mock('@auth0/auth0-react', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@auth0/auth0-react')>()),
  Auth0Provider: vi.fn(({ children }) => children),
  useAuth0: vi.fn(),
}))

type Auth0State = ReturnType<typeof useAuth0>

const settings = {
  domain: 'buckl-dev.us.auth0.com',
  clientId: 'client-123',
  audience: 'https://api.buckl.app',
}

let auth0: {
  isLoading: boolean
  isAuthenticated: boolean
  user?: { name?: string; email?: string }
  error?: Error
  loginWithRedirect: ReturnType<typeof vi.fn>
  logout: ReturnType<typeof vi.fn>
  getAccessTokenSilently: ReturnType<typeof vi.fn>
}

let session: Session

function Probe({ onSession }: { onSession: (session: Session) => void }) {
  const current = useSession()
  useEffect(() => {
    onSession(current)
  }, [current, onSession])
  return <p>{JSON.stringify(current.state)}</p>
}

function renderProvider(onSignedIn = vi.fn()) {
  vi.mocked(useAuth0).mockReturnValue(auth0 as unknown as Auth0State)
  render(
    <Auth0SessionProvider settings={settings} onSignedIn={onSignedIn}>
      <Probe onSession={(current) => (session = current)} />
    </Auth0SessionProvider>,
  )
  return onSignedIn
}

function providerProps(): ComponentProps<typeof Auth0Provider> {
  return vi.mocked(Auth0Provider).mock.calls[0][0]
}

describe('Auth0SessionProvider', () => {
  beforeEach(() => {
    vi.mocked(Auth0Provider).mockClear()
    auth0 = {
      isLoading: false,
      isAuthenticated: false,
      loginWithRedirect: vi.fn().mockResolvedValue(undefined),
      logout: vi.fn().mockResolvedValue(undefined),
      getAccessTokenSilently: vi.fn().mockResolvedValue('access-token'),
    }
  })
  afterEach(() => cleanup())

  it('configures Auth0 for the tenant, the API audience and rotating refresh tokens', () => {
    renderProvider()

    expect(providerProps()).toMatchObject({
      domain: 'buckl-dev.us.auth0.com',
      clientId: 'client-123',
      authorizationParams: {
        redirect_uri: `${window.location.origin}/callback`,
        audience: 'https://api.buckl.app',
      },
      cacheLocation: 'localstorage',
      useRefreshTokens: true,
      useRefreshTokensFallback: false,
    })
  })

  it.each([
    ['loading', { isLoading: true }, { status: 'loading' }],
    ['signed out', {}, { status: 'signedOut' }],
    [
      'signed in',
      { isAuthenticated: true, user: { name: 'Alice', email: 'alice@buckl.test' } },
      { status: 'signedIn', user: { displayName: 'Alice' } },
    ],
    [
      'signed in without a name',
      { isAuthenticated: true, user: { email: 'bob@buckl.test' } },
      { status: 'signedIn', user: { displayName: 'bob@buckl.test' } },
    ],
    [
      'after a failed sign-in',
      { error: new Error('access_denied') },
      { status: 'signedOut', error: { message: 'We could not sign you in. Please try again.' } },
    ],
  ])('reports the session as %s', (_label, state, expected) => {
    Object.assign(auth0, state)

    renderProvider()

    expect(screen.getByText(JSON.stringify(expected))).toBeInTheDocument()
  })

  it('signs in asking Auth0 to come back to a safe in-app path', async () => {
    renderProvider()

    await session.signIn('/wardrobe/g-1')
    await session.signIn('https://evil.example')

    expect(auth0.loginWithRedirect).toHaveBeenNthCalledWith(1, {
      appState: { returnTo: '/wardrobe/g-1' },
    })
    expect(auth0.loginWithRedirect).toHaveBeenNthCalledWith(2, {
      appState: { returnTo: '/wardrobe' },
    })
  })

  it('hands the safe return path to the app when Auth0 sends the user back', () => {
    const onSignedIn = renderProvider()

    providerProps().onRedirectCallback?.({ returnTo: '/wardrobe/g-1' })
    providerProps().onRedirectCallback?.({ returnTo: '//evil.example' })
    providerProps().onRedirectCallback?.(undefined)

    expect(onSignedIn.mock.calls).toEqual([['/wardrobe/g-1'], ['/wardrobe'], ['/wardrobe']])
  })

  it('signs out back to the welcome screen', async () => {
    renderProvider()

    await session.signOut()

    expect(auth0.logout).toHaveBeenCalledWith({
      logoutParams: { returnTo: `${window.location.origin}/welcome` },
    })
  })

  it('returns the access token Auth0 renews silently', async () => {
    renderProvider()

    await expect(session.getAccessToken()).resolves.toBe('access-token')
  })

  it.each(['login_required', 'invalid_grant', 'missing_refresh_token'])(
    'reports %s as an expired session',
    async (code) => {
      auth0.getAccessTokenSilently.mockRejectedValue(new GenericError(code, 'Log in again'))
      renderProvider()

      await expect(session.getAccessToken()).rejects.toBeInstanceOf(SessionExpiredError)
    },
  )

  it('reports a missing token as an expired session', async () => {
    auth0.getAccessTokenSilently.mockResolvedValue(undefined)
    renderProvider()

    await expect(session.getAccessToken()).rejects.toBeInstanceOf(SessionExpiredError)
  })

  it('lets any other token failure through unchanged', async () => {
    const offline = new TypeError('Failed to fetch')
    auth0.getAccessTokenSilently.mockRejectedValue(offline)
    renderProvider()

    await expect(session.getAccessToken()).rejects.toBe(offline)
  })
})
